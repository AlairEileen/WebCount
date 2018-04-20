using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Response.Json;
using Tools.Strings;
using WebCommon.Controllers;
using WebCount.AppDatas;
using WebCount.Models;

namespace WebCount.Controllers
{
    public class CountController : BaseController<CountData, CountModel>
    {
        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <param name="appID">appID</param>
        /// <param name="appSecret">appSecret</param>
        /// <returns></returns>
        public IActionResult GetAccessToken(string appID, string appSecret)
        {
            if (StringExtensions.CheckEmpty(appID, appSecret))
            {
                return this.JsonOtherStatus(Tools.Response.ResponseStatus.请求参数不正确);
            }
            return DoData(appID, RequestClientType.Api, null, (uniacid, aid) =>
            {
                return thisData.GetAccessToken(uniacid, appID, appSecret).ToJsonSuccess(this);
            });

        }
        public IActionResult GetAccessTokenByID()
        {
            return DoData("", RequestClientType.Api, null, (uniacid, aid) =>
            {
                return thisData.GetAccessToken(uniacid).ToJsonSuccess(this);
            });
        }




    }
}
