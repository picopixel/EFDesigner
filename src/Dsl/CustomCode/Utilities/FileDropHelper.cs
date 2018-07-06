﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{

   internal class FileDropHelper
   {
      public static void HandleDrop(Store store, string filename)
      {
         if (string.IsNullOrEmpty(filename))
            return;

         // read the file
         string fileContents = File.ReadAllText(filename);

         try
         {
            // parse the contents
            SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContents);

            if (tree.GetRoot() is CompilationUnitSyntax root)
            {
               // find namespace-less classes
               foreach (ClassDeclarationSyntax cls in root.Members.OfType<ClassDeclarationSyntax>())
               {
                  if (cls.BaseList != null && cls.BaseList.Types.FirstOrDefault()?.ToString() == "DbContext")
                     ProcessContext(store, cls);
                  else
                     ProcessClass(store, cls);
               }

               // same with enums
               foreach (EnumDeclarationSyntax en in root.Members.OfType<EnumDeclarationSyntax>())
                  ProcessEnum(store, en);


               // find classes and enums in namespaces
               foreach (NamespaceDeclarationSyntax ns in root.Members.OfType<NamespaceDeclarationSyntax>())
               {
                  foreach (ClassDeclarationSyntax cls in ns.Members.OfType<ClassDeclarationSyntax>())
                  {
                     if (cls.BaseList != null && cls.BaseList.Types.FirstOrDefault()?.ToString() == "DbContext")
                        ProcessContext(store, cls, ns);
                     else
                        ProcessClass(store, cls, ns);
                  }

                  foreach (EnumDeclarationSyntax en in ns.Members.OfType<EnumDeclarationSyntax>())
                     ProcessEnum(store, en, ns);
               }
            }
         }
         catch
         {
            ErrorDisplay.Show("No class or enum found in " + filename);
         }
      }

      private static void ProcessContext(Store store, ClassDeclarationSyntax ctx, NamespaceDeclarationSyntax ns = null)
      {

      }

      private static void ProcessEnum(Store store, EnumDeclarationSyntax en, NamespaceDeclarationSyntax ns = null)
      {
      }

      private static void ProcessClass(Store store, ClassDeclarationSyntax cls, NamespaceDeclarationSyntax ns = null)
      {
         ModelRoot modelRoot = store.ElementDirectory.AllElements.OfType<ModelRoot>().FirstOrDefault();
         string className = cls.Identifier.Text;

         #region Sanity checks

         // can't add duplicate class names
         if (store.ElementDirectory.AllElements.OfType<ModelClass>().Any(c => c.Name == className))
         {
            ErrorDisplay.Show($"'{className}' already exists in model.");
            return;
         }

         if (cls.TypeParameterList != null)
         {
            ErrorDisplay.Show($"Can't add generic class '{className}'.");
            return;
         }

         #endregion

         using (Transaction tx = store.TransactionManager.BeginTransaction("Process dropped class"))
         {
            ModelClass newClass = new ModelClass(store, new PropertyAssignment(ModelClass.NameDomainPropertyId, className));
            modelRoot.Types.Add(newClass);

            #region Namespace

            if (ns != null)
            {
               string @namespace = ns.Members.OfType<QualifiedNameSyntax>().FirstOrDefault()?.ToString();

               if (!string.IsNullOrEmpty(@namespace))
                  newClass.Namespace = @namespace;
            }

            #endregion

            if (cls.DescendantNodes().Any(n => n.Kind() == SyntaxKind.AbstractKeyword))
               newClass.IsAbstract = true;

            #region Base classes and interfaces

            if (cls.BaseList != null)
            {
               List<string> customInterfaces = new List<string>();

               foreach (BaseTypeSyntax type in cls.BaseList.Types)
               {
                  string baseName = type.ToString();

                  if (baseName == "INotifyPropertyChanged")
                  {
                     newClass.ImplementNotify = true;

                     continue;
                  }

                  // if we see the base class in the model, it's a superclass. Otherwise, it's a custom interface
                  ModelClass superClass = modelRoot.Types.OfType<ModelClass>().FirstOrDefault(c => c.Name == baseName);

                  if (superClass != null)
                     newClass.Superclass = superClass;
                  else
                     customInterfaces.Add(baseName);
               }

               newClass.CustomInterfaces = customInterfaces.Any()
                                              ? string.Join(",", customInterfaces)
                                              : null;
            }

            #endregion Base classes and interfaces

            #region Comments

            List<DocumentationCommentTriviaSyntax> xmlTrivia = cls.GetLeadingTrivia()
                                                                  .Select(i => i.GetStructure())
                                                                  .OfType<DocumentationCommentTriviaSyntax>()
                                                                  .ToList();

            foreach (DocumentationCommentTriviaSyntax xmlComment in xmlTrivia)
            {
               XmlElementSyntax summary = xmlComment.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault(x => x.StartTag.Name.ToString() == "summary");

               if (summary != null)
               {
                  newClass.Summary = string.Empty;

                  for (int index = 0; index < summary.Content.Count; index++)
                  {
                     XmlNodeSyntax xmlNodeSyntax = summary.Content[index];

                     newClass.Summary += (index == 0
                                             ? xmlNodeSyntax.ToString()
                                             : $"\n<p>{xmlNodeSyntax.ToString()}</p>");
                  }
               }

               XmlElementSyntax remarks = xmlComment.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault(x => x.StartTag.Name.ToString() == "remarks");

               if (remarks != null)
               {
                  newClass.Description = string.Empty;

                  for (int index = 0; index < remarks.Content.Count; index++)
                  {
                     XmlNodeSyntax xmlNodeSyntax = remarks.Content[index];

                     newClass.Description += (index == 0
                                                 ? xmlNodeSyntax.ToString()
                                                 : $"\n<p>{xmlNodeSyntax.ToString()}</p>");
                  }
               }
            }

            #endregion Comments

            #region Properties

            newClass.Attributes.Clear();

            foreach (string source in cls.Members.OfType<PropertyDeclarationSyntax>().Select(prop => prop.ToString()).Select(source => Regex.Replace(source, @"\[[^]]+\]", "", RegexOptions.Multiline).Replace("\r", "").Replace("\n", "")))
            {
               try
               {
                  ParseResult parseResult = ModelAttribute.Parse(modelRoot, source);

                  if (parseResult == null)
                  {
                     WarningDisplay.Show($"Could not parse '{source}'. The line will be discarded.");

                     continue;
                  }

                  string message = null;

                  if (string.IsNullOrEmpty(parseResult.Name) || !CodeGenerator.IsValidLanguageIndependentIdentifier(parseResult.Name))
                     message = $"Could not add '{parseResult.Name}' to {className}: '{parseResult.Name}' is not a valid .NET identifier";
                  else if (newClass.AllAttributes.Any(x => x.Name == parseResult.Name))
                     message = $"Could not add {parseResult.Name} to {className}: {parseResult.Name} already in use";
                  else if (newClass.AllNavigationProperties().Any(p => p.PropertyName == parseResult.Name))
                     message = $"Could not add {parseResult.Name} to {className}: {parseResult.Name} already in use";

                  if (message != null)
                  {
                     WarningDisplay.Show(message);

                     continue;
                  }

                  ModelAttribute modelAttribute = new ModelAttribute(store, new PropertyAssignment(ModelAttribute.NameDomainPropertyId, parseResult.Name), new PropertyAssignment(ModelAttribute.TypeDomainPropertyId, parseResult.Type ?? "String"), new PropertyAssignment(ModelAttribute.RequiredDomainPropertyId, parseResult.Required ?? true), new PropertyAssignment(ModelAttribute.MaxLengthDomainPropertyId, parseResult.MaxLength ?? 0), new PropertyAssignment(ModelAttribute.InitialValueDomainPropertyId, parseResult.InitialValue), new PropertyAssignment(ModelAttribute.IsIdentityDomainPropertyId, parseResult.IsIdentity), new PropertyAssignment(ModelAttribute.SetterVisibilityDomainPropertyId, parseResult.SetterVisibility ?? SetterAccessModifier.Public));
                  newClass.Attributes.Add(modelAttribute);
               }
               catch
               {
                  WarningDisplay.Show($"Could not parse '{source}'. The line will be discarded.");
               }
            }

            #endregion Properties

            tx.Commit();
         }
      }
   }

   /*
   internal class DropFileParser
   {
      private ModelClass _modelClass;
      private ModelRoot _modelRoot;
      private Store _store;

      public DropFileParser(Store store, string filename)
      {
         if (string.IsNullOrEmpty(filename))
            return;

         _modelRoot = store.ElementDirectory.AllElements.OfType<ModelRoot>().FirstOrDefault();
         _store = store;

         string fileContents = File.ReadAllText(filename);

         try
         {
            // parse the contents
            SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContents);

            if (tree.GetRoot() is CompilationUnitSyntax root)
            {
               ProcessNamespaces(root.Members);
               ProcessClasses(root.Members, null);
               ProcessEnums(root.Members, null);
            }
         }
         catch (Exception e)
         {
            ErrorMessage = $"Error processing {filename}: {e.Message}";
         }
      }

      public string ErrorMessage { get; set; }
      public bool HasError => ErrorMessage != null;

      private void ProcessClasses(SyntaxList<MemberDeclarationSyntax> nodes, NamespaceDeclarationSyntax @namespace)
      {
         foreach (ClassDeclarationSyntax classDeclaration in nodes.OfType<ClassDeclarationSyntax>())
         {

         }
      }

      private void ProcessProperties(SyntaxList<MemberDeclarationSyntax> nodes, ClassDeclarationSyntax @class)
      {
         foreach (PropertyDeclarationSyntax propertyDeclaration in nodes.OfType<PropertyDeclarationSyntax>())
         {

         }
      }

      private void ProcessNamespaces(SyntaxList<MemberDeclarationSyntax> nodes)
      {
         List<NamespaceDeclarationSyntax> namespaceDeclarations = nodes.OfType<NamespaceDeclarationSyntax>().ToList();

         foreach (NamespaceDeclarationSyntax namespaceDeclaration in namespaceDeclarations)
         {
            ProcessClasses(namespaceDeclaration.Members, namespaceDeclaration);
            ProcessEnums(namespaceDeclaration.Members, namespaceDeclaration);
         }
      }

      private void ProcessEnums(SyntaxList<MemberDeclarationSyntax> nodes, NamespaceDeclarationSyntax @namespace)
      {
         foreach (EnumDeclarationSyntax enumDeclaration in nodes.OfType<EnumDeclarationSyntax>())
         {
         }
      }

      private void ProcessEnumValues(SyntaxList<MemberDeclarationSyntax> nodes, EnumDeclarationSyntax @enum)
      {

      }

   }
*/
}
