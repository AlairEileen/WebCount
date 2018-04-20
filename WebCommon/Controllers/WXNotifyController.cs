using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tools.DB;
using Tools.Models;
using WXSmallAppCommon.WXTool;

namespace WebCommon.Controllers
{
    public class WXNotifyController : Controller
    {
        public WXNotifyController(IWeChatNotifyService weChatNotifyListener)
        {
            this.weChatNotifyListener = weChatNotifyListener;
        }
        private IWeChatNotifyService weChatNotifyListener;
        /// <summary>
        /// 微信支付回掉
        /// </summary>
        /// <returns></returns>
        public string OnWXPayBack()
        {
            //var body = Request.Body;

            //var builder = new StringBuilder();
            //using (Stream ins = body)
            //{
            //    var count = 0;
            //    var buffer = new byte[1024];
            //    while ((count = body.Read(buffer, 0, 1024)) > 0)
            //    {
            //        builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            //    }
            //}
            string builder = new StreamReader(Request.Body).ReadToEnd();

            //var bodyString = body.ToString();
            var bodyString = builder.ToString();

            Log.Info(this.GetType().ToString(), "Receive data from WeChat : " + bodyString);
            var ret = "";
            //转换数据格式并验证签名
            var data = new WxPayData();
            try
            {
                data.FromXml(bodyString, x =>
                {
                    var key = We7Tools.Models.We7ProcessMiniConfig.GetAllConfig(x).KEY;
                    return key;
                });
                OnPaySuccess(data);
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                var res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                ret = res.ToXml();
            }
            Log.Info(this.GetType().ToString(), "Check sign success");
            return ret;
        }

        /// <summary>
        /// 微信支付成功返回数据
        /// </summary>
        /// <param name="data"></param>
        private void OnPaySuccess(WxPayData data)
        {

            var attach = (string)data.GetValue("attach");
            var wxOrderId = (string)data.GetValue("transaction_id");
            if (string.IsNullOrEmpty(attach))
            {
                var em = new ExceptionModel { Content = "微信支付返回：attach为空" };
                em.Save();
                throw em;
            }
            if (string.IsNullOrEmpty(wxOrderId))
            {
                var em = new ExceptionModel { Content = "微信支付返回：微信订单号为空" };
                em.Save();
                throw em;
            }
            var aa = attach.Split(',');
            var uniacid = aa[0];
            var accountID = aa[1];
            var orderID = aa[2];
            var orderType = (OrderType)Convert.ToInt32(aa[3]);
            switch (orderType)
            {
                case OrderType.LessonOrder:
                    NotifyLessonOrder(uniacid, accountID, orderID, wxOrderId);
                    break;
                default:
                    break;
            }
        }

        private void NotifyLessonOrder(string uniacid, string accountID, string orderID, string wxOrderId)
        {
            if (weChatNotifyListener != null)
                weChatNotifyListener.NotifyLessonOrder(uniacid, accountID, orderID, wxOrderId);
        }

        //private void NotifyVipOrder(MongoDBTool mongo, string uniacid, string accountID, string orderID, string wxOrderId)
        //{
        //    var accountCollection = mongo.GetMongoCollection<AccountModel>();

        //    var filter = Builders<AccountModel>.Filter;
        //    var filterSum = filter.Eq(x => x.AccountID, new ObjectId(accountID)) & filter.Eq("VipOrders.OrderID", new ObjectId(orderID));
        //    var account = accountCollection.Find(filterSum).FirstOrDefault();
        //    var vipOrder = account.VipOrders.Find(x => x.OrderID.Equals(new ObjectId(orderID)) && !x.IsPaid);

        //    if (vipOrder == null)
        //    {
        //        var em = new ExceptionModel { Content = "微信支付返回：订单不存在或者已经生成" };
        //        em.Save();
        //        throw em;
        //    }
        //    var vipInfo = new Vip { VipMonths = vipOrder.VipDetail.VipMonths, VipType = vipOrder.VipDetail.VipType, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMonths(vipOrder.VipDetail.VipMonths) };
        //    var update = Builders<AccountModel>.Update
        //        .Set("VipOrders.$.WeChatOrderID", wxOrderId)
        //        .Set("VipOrders.$.IsPaid", true)
        //        .Set("VipOrders.$.PaidTime", DateTime.Now)
        //        .Set(x => x.VipInfo, vipInfo);

        //    accountCollection.UpdateOne(filterSum, update);
        //}

        //private void NotifyQROrder(MongoDBTool mongo, string uniacid, string accountID, string orderID, string wxOrderId)
        //{

        //    var accountCollection = mongo.GetMongoCollection<AccountModel>();

        //    var filter = Builders<AccountModel>.Filter;
        //    var filterSum = filter.Eq(x => x.AccountID, new ObjectId(accountID)) & filter.Eq("Orders.OrderID", new ObjectId(orderID));
        //    var account = accountCollection.Find(filterSum).FirstOrDefault();
        //    var wcOrder = account.Orders.Find(x => x.OrderID.Equals(new ObjectId(orderID)) && !x.IsPaid);

        //    if (wcOrder == null)
        //    {
        //        var em = new ExceptionModel { Content = "微信支付返回：订单不存在或者已经生成" };
        //        em.Save();
        //        throw em;
        //    }
        //    var update = Builders<AccountModel>.Update
        //        .Set("Orders.$.WeChatOrderID", wxOrderId)
        //        .Set("Orders.$.PaidTime", DateTime.Now)
        //        .Set("Orders.$.IsPaid", true);

        //    accountCollection.UpdateOne(filterSum, update);
        //}
    }
    public interface IWeChatNotifyService
    {
        void NotifyLessonOrder(string uniacid, string accountID, string orderID, string wxOrderId);
    }
}
