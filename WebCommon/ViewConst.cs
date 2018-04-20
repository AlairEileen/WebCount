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
        登录 = -1, 视频教程 = 0, 系统管理 = 1, 错误 = 2, 课程 = 3, 资讯 = 4,用户管理=5
    }
    public enum ErrorType
    {
        ErrorNoUserOrTimeOut = 0
    }
}
