﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBulkTools.TestCommon.Model;
using SqlBulkTools.TestCommon;

namespace SqlBulkTools.UnitTests
{
    [TestClass]
    public class DataTableOperationsTests
    {
        [TestMethod]
        public void DataTableTools_GetColumn_RetrievesColumn()
        {
            // Arrange
            DataTableOperations dtOps = new DataTableOperations();

            dtOps.SetupDataTable<Book>()
                .ForCollection(null)
                .AddColumn(x => x.ISBN)
                .AddColumn(x => x.Price)
                .PrepareDataTable();

            var expected1 = "ISBN";
            var expected2 = "Price";

            // Act
            var result1 = dtOps.GetColumn<Book>(x => x.ISBN);
            var result2 = dtOps.GetColumn<Book>(x => x.Price);

            // Assert
            Assert.AreEqual(expected1, result1);
            Assert.AreEqual(expected2, result2);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlBulkToolsException))]
        public void DataTableTools_GetColumn_ThrowSqlBulkToolsExceptionWhenNoSetup()
        {
            // Arrange
            DataTableOperations dtOps = new DataTableOperations();

            // Act and Assert
            dtOps.GetColumn<Book>(x => x.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlBulkToolsException))]
        public void DataTableTools_GetColumn_ThrowSqlBulkToolsExceptionWhenTypeMismatch()
        {
            // Arrange
            DataTableOperations dtOps = new DataTableOperations();
            dtOps.SetupDataTable<Book>()
                .ForCollection(new List<Book>() { new Book() { Description = "A book" } })
                .AddAllColumns()
                .PrepareDataTable();

            // Act and Assert
            dtOps.GetColumn<BookDto>(x => x.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlBulkToolsException))]
        public void DataTableTools_GetColumn_ThrowSqlBulkToolsExceptionWhenColumnMappingNotFound()
        {
            // Arrange
            DataTableOperations dtOps = new DataTableOperations();

            dtOps.SetupDataTable<Book>()
                .ForCollection(null)
                .AddColumn(x => x.ISBN)
                .AddColumn(x => x.Price)
                .PrepareDataTable();

            // Act and Assert
            dtOps.GetColumn<Book>(x => x.Description);
        }

        [TestMethod]
        public void DataTableTools_GetColumn_CustomColumnMapsCorrectly()
        {
            // Arrange
            var expected = "PublishingDate";
            DataTableOperations dtOps = new DataTableOperations();

            dtOps.SetupDataTable<Book>()
                .ForCollection(null)
                .AddAllColumns()
                .CustomColumnMapping(x => x.PublishDate, expected)
                .PrepareDataTable();

            // Act
            var result = dtOps.GetColumn<Book>(x => x.PublishDate);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlBulkToolsException))]
        public void DataTableTools_GetColumn_WhenColumnRemovedFromSetup()
        {
            // Arrange
            DataTableOperations dtOps = new DataTableOperations();

            dtOps.SetupDataTable<Book>()
                .ForCollection(null)
                .AddAllColumns()
                .RemoveColumn(x => x.Description)
                .PrepareDataTable();

            // Act and Assert
            dtOps.GetColumn<Book>(x => x.Description);
        }

        [TestMethod]
        public void DataTableTools_PrepareDataTable_WithThreeColumnsAdded()
        {
            BookRandomizer randomizer = new BookRandomizer();

            DataTableOperations dtOps = new DataTableOperations();
            List<Book> books = randomizer.GetRandomCollection(30);

            var dt = dtOps.SetupDataTable<Book>()
                .ForCollection(books)
                .AddColumn(x => x.ISBN)
                .AddColumn(x => x.Price)
                .AddColumn(x => x.PublishDate)
                .CustomColumnMapping(x => x.PublishDate, "SomeOtherMapping")
                .PrepareDataTable();

            Assert.AreEqual("ISBN", dt.Columns[dtOps.GetColumn<Book>(x => x.ISBN)].ColumnName);
            Assert.AreEqual("Price", dt.Columns[dtOps.GetColumn<Book>(x => x.Price)].ColumnName);
            Assert.AreEqual("SomeOtherMapping", dt.Columns[dtOps.GetColumn<Book>(x => x.PublishDate)].ColumnName);
            Assert.AreEqual(typeof(DateTime), dt.Columns[dtOps.GetColumn<Book>(x => x.PublishDate)].DataType);
        }

        [TestMethod]
        public void DataTableTools_BuildPreparedDataDable_AddsRows()
        {
            var rowCount = 30;
            BookRandomizer randomizer = new BookRandomizer();

            DataTableOperations dtOps = new DataTableOperations();
            List<Book> books = randomizer.GetRandomCollection(rowCount);

            var dt = dtOps.SetupDataTable<Book>()
                .ForCollection(books)
                .AddAllColumns()
                .PrepareDataTable();

            dt = dtOps.BuildPreparedDataDable();

            Assert.AreEqual(rowCount, dt.Rows.Count);
            Assert.AreEqual(books[10].ISBN, dt.Rows[10].Field<string>(dtOps.GetColumn<Book>(x => x.ISBN)));
            Assert.AreEqual(books[10].Description, dt.Rows[10].Field<string>(dtOps.GetColumn<Book>(x => x.Description)));
        }

        [TestMethod]
        public void DataTableTools_BuildPreparedDataDable_WithCustomDataTableSettings()
        {
            long autoIncrementSeedTest = 21312;
            BookRandomizer randomizer = new BookRandomizer();

            DataTableOperations dtOps = new DataTableOperations();
            List<Book> books = randomizer.GetRandomCollection(30);

            var dt = dtOps.SetupDataTable<Book>()
                .ForCollection(books)
                .AddAllColumns()
                .PrepareDataTable();

            dt.Columns[dtOps.GetColumn<Book>(x => x.Id)].AutoIncrementSeed = autoIncrementSeedTest;

            dt = dtOps.BuildPreparedDataDable();

            Assert.AreEqual(dt.Columns[dtOps.GetColumn<Book>(x => x.Id)].AutoIncrementSeed, autoIncrementSeedTest);

        }
    }
}
