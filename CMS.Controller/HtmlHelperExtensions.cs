using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Web.Mvc.Html
{
	public static class HtmlHelperExtensions
	{
		/// <summary>
		/// 在EditorFor中的DropDownListFor採用此方法，否則不會加載model數據
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="htmlHelper"></param>
		/// <param name="expression"></param>
		/// <param name="selectList"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static MvcHtmlString EditorDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
		{
			var dropDown = SelectExtensions.DropDownListFor(htmlHelper, expression, selectList, htmlAttributes);
			var model = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model;
			if (model == null)
			{
				return dropDown;
			}
			var dropDownWithSelect = dropDown.ToString().Replace("value=\"" + model.ToString() + "\"", "value=\"" + model.ToString() + "\" selected");
			return MvcHtmlString.Create(dropDownWithSelect);
		}

		/// <summary>
		/// 帶時間插件的INPUT輸入框
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="htmlHelper"></param>
		/// <param name="expression"></param>
		/// <param name="htmlAttributes"></param>
		/// <param name="formatString">時間格式化字符串</param>
		/// <returns></returns>
		public static MvcHtmlString DateTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, string formatString)
		{
			var input = InputExtensions.TextBoxFor(htmlHelper, expression, htmlAttributes);
			var model = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model;
			if (model == null)
			{
				return input;
			}
			string dateStr = ((DateTime)model).ToString(formatString);
			if (dateStr.Contains("0001")) dateStr = "";
			var inputWithDate = input.ToString().Replace("value=\"" + model.ToString() + "\"", "value=\"" + dateStr + "\"");
			return MvcHtmlString.Create(inputWithDate);
		}
	}
}
