using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCount.AppDatas;
using WebCount.Models;

namespace WebCount.Services
{
    public class AccessTokenRefreshService
    {
        private static int timeSpan = 6000 * 1000;
        private static CountData countData;
        private static Timer timer;
        public static void StartRefresh()
        {
            if (countData == null)
            {
                countData = new CountData();
            }
            timer = new Timer(countData.RefreshAccessToken, null, 1000 * 10, timeSpan);
        }

    }
    public static class AccessTokenRefreshExtension
    {
        public static void AddAccessTokenRefresh(this IServiceCollection services)
        {

            AccessTokenRefreshService.StartRefresh();
        }
    }
}
