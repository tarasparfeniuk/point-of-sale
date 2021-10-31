﻿using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PointOfSale.Results;
using PointOfSale.Storage;

namespace PointOfSale.Tests
{
    [TestFixture]
    public class PointOfSaleTerminalSessionTests
    {
        private readonly MockRepository m_mockRepository;
        private readonly Mock<ISaleItemDataProvider> m_dataProviderMock;
        
        private ISaleItemDataProvider m_dataProvider;

        public PointOfSaleTerminalSessionTests()
        {
            m_mockRepository = new MockRepository(MockBehavior.Strict);
            m_dataProviderMock = m_mockRepository.Create<ISaleItemDataProvider>();
        }

        [SetUp]
        public void Setup()
        {
            foreach (var item in GetTestSaleItems())
            {
                m_dataProviderMock
                    .Setup(s => s.GetByCode(item.Code))
                    .Returns(Result<SaleItem>.SuccessResult(item));
            }

            m_dataProvider = m_dataProviderMock.Object;
        }

        [Test]
        [TestCase("ABCDABA", 13.25)]
        [TestCase("CCCCCCC", 6.00)]
        [TestCase("ABCD", 7.25)]
        public void PointOfSaleTerminalSession_SequenceOfCodes_ShouldCalculateCorrectTotal(string inputCodes, decimal expectedTotal)
        {
            var target = new PointOfSaleTerminalSession(m_dataProvider);

            var codes = inputCodes.Select(c => c.ToString());

            foreach (var code in codes)
            {
                target.Scan(code);
            }

            var totalResult = target.CalculateTotal();

            Assert.IsTrue(totalResult.Success);
            Assert.AreEqual(expectedTotal, totalResult.Value);
        }

        private IEnumerable<SaleItem> GetTestSaleItems()
        {
            var itemA = new SaleItem("A", new decimal(1.25));
            itemA.SetPrice(3, new decimal(3.00));
            yield return itemA;

            var itemB = new SaleItem("B", new decimal(4.25));
            yield return itemB;

            var itemC = new SaleItem("C", new decimal(1.00));
            itemC.SetPrice(6, new decimal(5.00));
            yield return itemC;

            var itemD = new SaleItem("D", new decimal(0.75));
            yield return itemD;
        }
    }
}