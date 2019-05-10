using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication6.Models;

namespace WebApplication6.Services
{
    public class TestDataService : ITestDataService
    {
        public TestDataService()
        {
        }

        public async Task<TestData> GetTestDataAsync()
        {
            return await Task.FromResult<TestData>(new TestData { Name = "Michael", Department = "IT", Saldo = 20.23 });
        }
    }
}
