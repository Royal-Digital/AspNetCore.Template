﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using MvcTemplate.Components.Security;
using MvcTemplate.Resources;
using MvcTemplate.Resources.Shared;
using NonFactors.Mvc.Grid;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MvcTemplate.Components.Extensions
{
    public static class MvcGridExtensions
    {
        public static IGridColumn<T, IHtmlContent> AddAction<T>(this IGridColumnsOf<T> columns, String action, String iconClass) where T : class
        {
            if (!IsAuthorizedFor(columns.Grid.ViewContext, action))
                return new GridColumn<T, IHtmlContent>(columns.Grid, model => null);

            return columns.Add(model => GenerateLink(columns.Grid.ViewContext, model, action, iconClass)).Css("action-cell");
        }

        public static IGridColumn<T, DateTime> AddDate<T>(this IGridColumnsOf<T> columns, Expression<Func<T, DateTime>> expression)
        {
            return columns.AddProperty(expression).Formatted("{0:d}");
        }
        public static IGridColumn<T, DateTime?> AddDate<T>(this IGridColumnsOf<T> columns, Expression<Func<T, DateTime?>> expression)
        {
            return columns.AddProperty(expression).Formatted("{0:d}");
        }
        public static IGridColumn<T, Boolean> AddBoolean<T>(this IGridColumnsOf<T> columns, Expression<Func<T, Boolean>> expression)
        {
            Func<T, Boolean> valueFor = expression.Compile();

            return columns
                .AddProperty(expression)
                .MultiFilterable(false)
                .RenderedAs(model => valueFor(model) ? Strings.Yes : Strings.No);
        }
        public static IGridColumn<T, Boolean?> AddBoolean<T>(this IGridColumnsOf<T> columns, Expression<Func<T, Boolean?>> expression)
        {
            Func<T, Boolean?> valueFor = expression.Compile();

            return columns
                .AddProperty(expression)
                .MultiFilterable(false)
                .RenderedAs(model =>
                    valueFor(model) != null
                        ? valueFor(model) == true
                            ? Strings.Yes
                            : Strings.No
                        : "");
        }
        public static IGridColumn<T, DateTime> AddDateTime<T>(this IGridColumnsOf<T> columns, Expression<Func<T, DateTime>> expression)
        {
            return columns.AddProperty(expression).Formatted("{0:g}");
        }
        public static IGridColumn<T, DateTime?> AddDateTime<T>(this IGridColumnsOf<T> columns, Expression<Func<T, DateTime?>> expression)
        {
            return columns.AddProperty(expression).Formatted("{0:g}");
        }
        public static IGridColumn<T, TProperty> AddProperty<T, TProperty>(this IGridColumnsOf<T> columns, Expression<Func<T, TProperty>> expression)
        {
            return columns
                .Add(expression)
                .Css(CssClassFor<TProperty>())
                .Titled(Resource.ForProperty(expression));
        }

        public static IHtmlGrid<T> ApplyDefaults<T>(this IHtmlGrid<T> grid)
        {
            return grid
                .Pageable(pager => { pager.RowsPerPage = 16; })
                .Empty(Strings.NoDataFound)
                .AppendCss("table-hover")
                .Filterable()
                .Sortable();
        }

        private static IHtmlContent GenerateLink<T>(ViewContext context, T model, String action, String iconClass)
        {
            TagBuilder link = new TagBuilder("a");
            link.AddCssClass(action.ToLower() + "-action");
            link.Attributes["href"] = new UrlHelper(context).Action(action, RouteFor(model));

            TagBuilder icon = new TagBuilder("span");
            icon.AddCssClass(iconClass);

            link.InnerHtml.AppendHtml(icon);

            return link;
        }
        private static Boolean IsAuthorizedFor(ViewContext context, String action)
        {
            IAuthorization authorization = context.HttpContext.RequestServices.GetService<IAuthorization>();
            if (authorization == null)
                return true;

            Int32? account = context.HttpContext.User.Id();
            String area = context.RouteData.Values["area"] as String;
            String controller = context.RouteData.Values["controller"] as String;

            return authorization.IsGrantedFor(account, area, controller, action);
        }
        private static IDictionary<String, Object> RouteFor<T>(T model)
        {
            PropertyInfo id = typeof(T).GetProperty("Id") ?? throw new Exception(typeof(T).Name + " type does not have an id.");

            return new Dictionary<String, Object> { ["id"] = id.GetValue(model) };
        }
        private static String CssClassFor<TProperty>()
        {
            Type type = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);
            if (type.IsEnum)
                return "text-left";

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return "text-right";
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                    return "text-center";
                default:
                    return "text-left";
            }
        }
    }
}
