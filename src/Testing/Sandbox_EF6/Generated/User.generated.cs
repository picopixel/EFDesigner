//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor
//     https://github.com/msawczyn/EFDesigner
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sandbox_EF6
{
   public partial class User
   {
      partial void Init();

      /// <summary>
      /// Default constructor. Protected due to required properties, but present because EF needs it.
      /// </summary>
      protected User()
      {
         Init();
      }

      /// <summary>
      /// Public constructor with required data
      /// </summary>
      /// <param name="role"></param>
      public User(global::Sandbox_EF6.Role role)
      {
         if (role == null) throw new ArgumentNullException(nameof(role));
         this.Role = role;

         Init();
      }

      /// <summary>
      /// Static create function (for use in LINQ queries, etc.)
      /// </summary>
      /// <param name="role"></param>
      public static User Create(global::Sandbox_EF6.Role role)
      {
         return new User(role);
      }

      /*************************************************************************
       * Persistent properties
       *************************************************************************/

      /// <summary>
      /// Identity, Required, Indexed
      /// </summary>
      [Key]
      [Required]
      public long Id { get; set; }

      /*************************************************************************
       * Persistent navigation properties
       *************************************************************************/

      /// <summary>
      /// Required{br/}
      /// Role
      /// </summary>
      /// <remarks>
      /// {p}While giving you complete control over how the code is generated you'll be able
      /// to, out of the box, create sophisticated,
      /// consistent and {strong}correct{/strong} Entity Framework code that can be regenerated
      /// when your model changes. And, since the code is written using
      /// partial classes, any additions you make to your generated code is retained across
      /// subsequent generations.{/p}
      /// {p}If you are used to the EF visual modeling that comes with Visual Studio, you'll
      /// be pretty much at home. The goal was to duplicate
      /// at least those features and, in addition, add all the little things that {em}should{/em}
      /// have been there. Things like:{/p}
      /// {ul}
      /// {li}the ability to show and hide parts of the model{/li}
      /// {li}easy customization of generated output by editing or even replacing the T4 templates{/li}
      /// {li}entities by default generated as partial classes so the generated code can be
      /// easily extended{/li}
      /// {li}class and enumeration nodes that can be colored to visually group the model{/li}
      /// {li}different concerns being generated into different subdirectories (entities, enums,
      /// dbcontext){/li}
      /// {li}string length, index flags, required attributes and other properties being available
      /// in the designer{/li}
      /// {/ul}
      /// </remarks>
      public virtual global::Sandbox_EF6.Role Role { get; set; }

   }
}

