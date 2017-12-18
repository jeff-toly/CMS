//按回車不提交表单
$(document).keydown(function (e) {
  e = e ? e : window.event;
  var keycode = e.which ? e.which : e.keyCode;
  var type = e.target.type;
  if (keycode == 13 && type != "textarea") {
    return false;
  }
});
//調用ajaxForm，保存文件后獲取返回的json結果
$(function () {
  var startStatus = true;
  $('form').ajaxForm({
    type: "post",
    dataType: "json",
    url: '<%: Url.Action("SavePersonal", "Personal")%>',
    beforeSubmit: function () {
      var validate = false;
      if (startStatus) {
        validate = CheckData();
        if (validate) startStatus = false;
      }
      if (!validate) return false;
    },
    success: function (json) {
      if (json != null) {
        if (json.status == true) {//保存成功
          if (json.fileId != "") {//如果修改了專長上傳文件，修改相應的下載鏈接
            var _aDownload = $("#LaborContractFile").parent().find("a");
            var _url = '<%=Url.Action("Download", "Global")%>';
            var _downloadPath = "DownloadFile(\"" + _url + "\", \"" + json.fileId + "\")";
            if (_aDownload.length != 0) {//已存在則修改，根據結果長度判斷是否存在（不能用null判斷）
              _aDownload.attr("onclick", _downloadPath);
            }
            else {//不存在則添加
              var _html = "<a href='#' onclick='" + _downloadPath + "'>下載文件</a>如需要改請點擊'瀏覽'按鈕重新選擇文件";
              $("#LaborContractFile").parent().append(_html);
            }
            $("#LaborContractFile").val("");
            $("[id$='LABORCONTRACT']").val(json.fileId);
          }
          layer.msg(json.message, 1, 1);
          startStatus = true;
        }
        else {//保存失敗
          layer.msg(json.message, 1, 3);
          startStatus = true;
        }
      }
      else {
        layer.msg("發生異常", 1, 3);
      }
    }
  });
});
//FORM提交時驗證數據，提示錯誤
function CheckData() {
  var flag = ValidationBeforeSave();
  if (flag) {
    $("#ShowErrorMsg").hide();
    layer.load("信息提交中，請稍後");
  }
  else {
    $("#ShowErrorMsg").show();
    $("#ShowErrorMsg").html("有必填信息未完善，請檢查每個頁面，完善後再提交");
  }
  return flag;
}
//圖片預覽：瀏覽器版本不同採用不同的方式
function ChangePicture(obj, imgId) {
  var preview = document.getElementById(imgId);
  var ieVersion = IEVersion();
  if (ieVersion == "IE5-9") {//IE8等瀏覽器使用IE濾鏡
    obj.select();
    path = document.selection.createRange().text;
    preview.style.display = "none"; //隐藏原图
    var previewDiv = document.getElementById(imgId + "Div");
    //設置滤镜DIV
    previewDiv.style.filter = "progid:DXImageTransform.Microsoft.AlphaImageLoader(enabled='true',sizingMethod='scale',alt='',src=\""
            + path + "\")";
  }
  else if (ieVersion == "IE11" || ieVersion == "IE10" || ieVersion == "NOT IE") {
    var file = obj.files[0];
    var reader = new FileReader();
    if (file) {
      reader.readAsDataURL(file);
    }
    else {
      preview.src = "";
    }
    reader.onload = function () {
      preview.src = this.result;
    }
  }
}
//獲取IE版本
function IEVersion() {
	var browser = navigator.appName;
	var b_version = navigator.appVersion;
	var version = b_version.split(";");
	if (version.length > 1) {
		var trim_Version = version[1].replace(/[ ]/g, "");
		if (browser == "Netscape" && trim_Version == "Trident/7.0")
			return "IE11";
		else if (browser == "Microsoft Internet Explorer" && trim_Version != "MSIE10.0")
			return "IE5-9";
		else if (browser == "Microsoft Internet Explorer")
			return "IE10";
	}
	return "NOT IE";
}
//加载时时重置BODY高度和宽度
$(function () {//同一個頁面可以有多個$(document).ready(function)（等價于$(function)）;
	if (window.dialogHeight) {
		$("body").height(parseInt(window.dialogHeight, 10) * 0.98);
		$("body").width(parseInt(window.dialogWidth, 10) * 0.98);
	}
});
//窗体大小改变时重置BODY高度和宽度
$(window).resize(function () {
	if (window.dialogHeight) {
		$("body").height(parseInt(window.dialogHeight, 10) * 0.98);
		$("body").width(parseInt(window.dialogWidth, 10) * 0.98);
	}
});
//移除空白
function trimAllText() {
	$(':text,textarea').each(function () {
		var value = $.trim(this.value);
		$(this).val(value);
	});
}
//提交前验证
function ValidationBeforeSave() {
	trimAllText();
	var flag = true;
	$('[class*=validation], .combo-text').each(function () {
		LoadValidation(this);
	})
	if ($('.validation-error').length > 0)
		flag = false;
	return flag;
}
//验证
function LoadValidation(obj) {
	if ($(obj).attr("class").indexOf('combobox-required') >= 0) {
		if (typeof $(obj).next().next('span.error-message').attr("class") != "undefined") {
			$(obj).next().removeClass('validation-error');
			$(obj).next().next('span.error-message').remove();
		}
	} else {
		if (typeof $(obj).next('span.error-message').attr("class") != "undefined") {
			$(obj).removeClass('validation-error');
			$(obj).next('span.error-message').remove();
		}
	}
	var rulesParsing = $(obj).attr('class');
	var rulesRegExp = /\[(.*)\]/;
	var rules = rulesRegExp.exec(rulesParsing);
	if (rules) {
		str = rules[1];
		pattern = /\[|,|\]/;
		rules = str.split(pattern);
		var errMsg = ValidateInput(obj, rules);
		if ($.trim(errMsg).length > 0) {
			if ($(obj).is('select') && $(obj).attr("class").indexOf('combobox-required') < 0)
				$(obj).css("border-color", "#D82C36 #DD303A #DD303A #D82C36");
			else if ($(obj).attr("class").indexOf('combobox-required') >= 0) {
				$(obj).next().css("border-color", "#D82C36 #DD303A #DD303A #D82C36");
				$(obj).next().addClass("validation-error");
				$(obj).next().after('<span class="error-message" style="display:none">' + errMsg + '</span>');
			}
			else
				$(obj).css("border-color", "#D82C36 #DD303A #DD303A #D82C36");

			if ($(obj).attr("class").indexOf('combobox-required') < 0) {
				$(obj).after('<span class="error-message" style="display:none">' + errMsg + '</span>');
				$(obj).addClass("validation-error");
			}
		}
		else {
			$(obj).removeClass("validation-error");
			if ($(obj).is('select') && $(obj).attr("class").indexOf('combobox-required') < 0)
				$(obj).css("border-color", "#9A9A9A #CDCDCD #CDCDCD #9A9A9A");
			else if ($(obj).attr("class").indexOf('combobox-required') >= 0)
				$(obj).next().css("border-color", "#9A9A9A #CDCDCD #CDCDCD #9A9A9A");
			else
				$(obj).css("border-color", "#9A9A9A #CDCDCD #CDCDCD #9A9A9A");
		}
	}
}
//验证
function ValidationEngine() {
	$('[class*=validation], .combo-text').each(function () {
		$(this).blur(function () {
			var obj = this;
			if ($(this).hasClass('combo-text'))
				obj = $(this).parent().prev();
			LoadValidation(obj);
		});
	});
}
//加载时验证
$(function () {
	$('select').each(function () {
		if ($(this).attr("class") != null && $(this).attr("class") != undefined && $(this).attr("class").indexOf('combobox-required') < 0)
			$(this).wrap('<span></span>');
	});
	ShowError();
	ValidationEngine();
});
//显示错误提示
function ShowError() {
	var x = 10; var y = 10;
	$('.validation-error').live('mouseover', function (e) {
		var errorMsg = $.trim($(this).next('span.error-message').html());
		var toolTip = "<div id='tooltip' style=\"font-size:12px;\">" + errorMsg + "</div>";
		$("body").append(toolTip);
		$("#tooltip").css({
			"position": "absolute",
			"padding": "5px",
			"background": "#ECB7B8",
			"border": "1px #FE2121 solid",
			"top": (e.pageY + y) + "px",
			"left": (e.pageX + x) + "px"
		}).show(200);
	}).live('mouseout', function () {
		$("#tooltip").remove();
	}).live('mousemove', function (e) {
		$("#tooltip").css({
			"background": "#ECB7B8",
			"padding": "5px",
			"border": "1px #FE2121 solid",
			"top": (e.pageY + y) + "px",
			"left": (e.pageX + x) + "px"
		})
	});
}
//验证输入框
function ValidateInput(obj, rules) {
	var errMsg = "";
	for (var i = 0; i < rules.length; i++) {
		var msg = "";
		switch (rules[i]) {
			case "required":
				msg = ValidateRequire(obj);
				break;
			case "combobox-required":
				msg = ValidateComboRequire(obj);
				break;
			case "email":
				msg = ValidateEmail(obj);
				break;
			case "pno":
				msg = ValidateUserNO(obj);
				break;
			case "length":
				msg = ValudateLength(obj, rules, i);
				break;
			case "date":
				msg = ValidateDate(obj);
				break;
			case "decimal":
				msg = ValidateDecimal(obj);
				break;
			case "decimalRequired":
				msg = ValidateDecimalRequired(obj);
				break;
			case "twodecimal":
				msg = ValidateTwoDecimal(obj);
				break;
			case "plasticNO":
				msg = ValidatePlasticNO(obj);
				break;
			case "chemicalNO":
				msg = ValidateChemicalNO(obj);
				break;
			case "metalSize":
				msg = ValidateMetalSize(obj);
				break;
			case "noSpechars":
				msg = ValidateSpechars(obj);
				break;
			case "readonlyFile":
				msg = ValidateExtension(obj);
				break;
			case "integerinfo": //非必填
				msg = ValidateIntegerinfo(obj);
				break;
			case "integerinfoRequired":  //必填
				msg = ValidateIntegerinfoRequired(obj);
				break;
			case "IntNeginfo": //非必填
				msg = ValidateIntNeginfo(obj);
				break;
			case "IntNeginfoRequired": //必填
				msg = ValidateIntNeginfoRequired(obj);
				break;
			case "select":
				msg = ValidateSelect(obj);
				break;
			default:
				break;
		}
		if (msg != "") 
		{
			errMsg += errMsg.length > 0 ? "<br>" + msg : msg;
		}
	}
	return errMsg;
}

//select選擇驗證，提示選擇，如果下拉列表有默認值（非“所有”）則不需要此驗證
function ValidateSelect(obj) {
	if ($(obj).get(0).selectedIndex == 0)
		return "*請選擇";
	else
		return "";
}

function ValidateIntNeginfo(obj) {
	var reg = /^-?(0|([1-9]\d*)|([1-9]\d{0,2}(,\d{3})*))(\.\d{0,3})?$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能输入正數或負數，小數位最多3位";
	else
		return "";
}

function ValidateIntNeginfoRequired(obj) {
	var reg = /^-?(0|([1-9]\d*)|([1-9]\d{0,2}(,\d{3})*))(\.\d{0,3})?$/;
	if (!reg.test($.trim($(obj).val())))
		return "*该项只能输入正數或負數，小數位最多3位";
	else
		return "";
}

//非負整數驗證，包括逗號分隔的正常數字字符串
function ValidateIntegerinfo(obj) {
	var reg = /^0$|^[1-9]\d*$|^[1-9]\d{0,2}(,\d{3})*$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能输入非負整数";
	else
		return "";
}

//非負整數驗證，包括逗號分隔的正常數字字符串
function ValidateIntegerinfoRequired(obj) {
	var reg = /^0$|^[1-9]\d*$|^[1-9]\d{0,2}(,\d{3})*$/;
	if (!reg.test($.trim($(obj).val())))
		return "*该项只能输入非負整数";
	else
		return "";
}

function ValidateRequire(obj) {
	if ($.trim($(obj).val()).length == 0)
		return "*该项为必填项";
	else
		return "";
}

function ValidateComboRequire(obj) {
	if ($.trim($(obj).next('.combo').find('.combo-value').val()).length == 0)
		return "*该项为必填项";
	else
		return "";
}

function ValidateEmail(obj) {
	//var reg = /^[a-zA-Z0-9_\s\.\-]+\@([a-zA-Z0-9\-]+\.)+[a-zA-Z0-9]{2,4}$/;
	var reg = /^[a-zA-Z0-9]+[\w\-]*\.?\w*\.?\w+\@(\w+\.)+\w{2,4}$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*邮件地址格式不正确。";
	else
		return "";
}

function ValidateDate(obj) {
	var reg = /^(?:(?!0000)[0-9]{4}\/(?:(?:0[1-9]|1[0-2])\/(?:0[1-9]|1[0-9]|2[0-8])|(?:0[13-9]|1[0-2])\/(?:29|30)|(?:0[13578]|1[02])\/31)|(?:[0-9]{2}(?:0[48]|[2468][048]|[13579][26])|(?:0[48]|[2468][048]|[13579][26])00)\/02\/29)$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*日期格式不正确。";
	else
		return "";
}

//非負數驗證，包括逗號分隔整數部分的非負數字符串
function ValidateDecimal(obj) {
	var reg = /^0$|^[1-9]\d*(\.\d+)?$|^[1-9]\d{0,2}(,\d{3})*(\.\d+)?$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能输入非負整数、逗號或小数";
	else
		return "";
}

//非負數驗證，包括逗號分隔整數部分的非負數字符串(必填項)
function ValidateDecimalRequired(obj) {
	var reg = /^0$|^[1-9]\d*(\.\d+)?$|^[1-9]\d{0,2}(,\d{3})*(\.\d+)?$/;
	if (!reg.test($.trim($(obj).val())))
		return "*该项只能输入非負整数、逗號或小数";
	else
		return "";
}

function ValidateTwoDecimal(obj) {
	var reg = /^\d+(\.\d{3})$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能为含三位小数的数字";
	else
		return "";
}
function ValidatePlasticNO(obj) {
	var reg = /^[A-Za-z\d\-]+$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能由字母、数字与‘-’组成";
	else
		return "";
}

function ValidateUserNO(obj) {
	var reg = /^[A-Za-z\d]+$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能由字母、数字组成";
	else
		return "";
}
function ValidateChemicalNO(obj) {
	var reg = /^(([\d\-]+)|(N\/A))$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能由数字与‘-’组成，若无內容请填写‘N/A’";
	else
		return "";
}
function ValidateMetalSize(obj) {
	var reg = /^.?\s*\d+(\.\d{3})$/;
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项只能由字符、含三位小数的数字组成";
	else
		return "";
}
function ValidateSpechars(obj) {
	var reg = new RegExp("[`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）&;—|{}【】‘；：”“'。，、？]");
	if ($.trim($(obj).val()).length != 0 && !reg.test($.trim($(obj).val())))
		return "*该项不能由特殊的字符组成";
	else
		return "";
}
function ValudateLength(obj, rules, position) {
	var startLength = eval(rules[position + 1]);
	var endLength = eval(rules[position + 2]);
	var feildLength = $.trim($(obj).val()).length;
	if (feildLength < startLength || feildLength > endLength)
		return "*请输入" + startLength + "至" + endLength + "个数字/字符";
	else
		return "";
}

//驗證附件格式以及附件是否存在（如果不存在，則要求上傳文件）
function ValidateExtension(obj) {
	var errMsg = "";
	if ($(obj).val() != null && $(obj).val() != "") {
		var objExtension = /\.[^\.]+$/.exec($(obj).val());
		if (objExtension != null) {
			var extension = objExtension.toString().toLowerCase();
			if (extension != ".xls" && extension != ".xlsx" && extension != ".doc" && extension != ".docx" && extension != ".ppt" && extension != ".pptx" && extension != ".txt" && extension != ".pdf")
				errMsg = "*不支持后缀名为" + extension + "的文件";
		} else
			errMsg = "*文件后缀名错误";
	}
	return errMsg;
}
//移除特殊字符
function StripScript(s) {
	var pattern = new RegExp("[`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）&;—|{}【】‘；：”“'。，、？]")
	var rs = "";
	for (var i = 0; i < s.length; i++) {
		rs = rs + s.substr(i, 1).replace(pattern, '');
	}
	return rs;
}
