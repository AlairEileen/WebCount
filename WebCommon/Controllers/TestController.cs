using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebCommon.Controllers
{
    public class TestController : Controller
    {
        private ITestService testService;
        public TestController(ITestService testService)
        {
            this.testService = testService;
        }
        public IActionResult GetText()
        {
            return Json(testService.GetText());
        }
    }
    public interface ITestService
    {
        string GetText();
    }
}
