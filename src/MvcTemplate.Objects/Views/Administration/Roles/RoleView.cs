﻿using MvcTemplate.Components.Extensions.Html;
using System;
using System.ComponentModel.DataAnnotations;

namespace MvcTemplate.Objects
{
    public class RoleView : BaseView
    {
        [Required]
        [StringLength(128)]
        public String Title { get; set; }

        public JsTree PrivilegesTree { get; set; }

        public RoleView()
        {
            PrivilegesTree = new JsTree();
        }
    }
}
