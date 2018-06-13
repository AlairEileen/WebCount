using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebCommon
{
    public class ViewConst
    {
        public static bool CheckRout(ViewDataDictionary<dynamic> viewData, RoutType routType)
        {
            return (RoutType)viewData["routType"] == routType;
        }
        public static void SetRoutType<T>(ViewDataDictionary<T> viewData, RoutType routType)
        {
            viewData["routType"] = routType;
            viewData["Title"] = routType.ToString();
        }

    }

    public enum RoutType
    {
        登录 = -1, 首页 = 0, 系统管理 = 1, 错误 = 2, 课程 = 3, 资讯 = 4, 小程序录入 = 5, 总体统计 = 6, 详细统计 = 7, 商户列表 = 8, 小程序列表 = 9,商户统计=10,小程序详细统计=11
    }
    public enum ErrorType
    {
        ErrorNoUserOrTimeOut = 0
    }
}
