using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SolrNet.Exceptions;

namespace SolrNet.Tests {
	[TestFixture]
	public class SolrOperationsTests {
		public class TestDocumentWithoutUniqueKey : ISolrDocument {}

		public class TestDocumentWithUniqueKey : ISolrDocument {
			[SolrUniqueKey]
			public int id {
				get { return 0; }
			}
		}

		[Test]
		public void Add() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<add><doc /></add>")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Add(new TestDocumentWithoutUniqueKey());
			});
		}

		[Test]
		public void Commit() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<commit />")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Commit();
			});
		}

		[Test]
		public void CommitWithOptions() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<commit waitSearcher=\"true\" waitFlush=\"true\" />")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Commit(true, true);
			});
		}

		[Test]
		public void DeleteByQuery() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<delete><query>id:123</query></delete>"))
					.Repeat.Once()
					.Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithUniqueKey>(connection);
				ops.Delete(new SolrQuery("id:123"));
			});
		}

		[Test]
		public void DeleteByQueryWithParams() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<delete fromPending=\"true\" fromCommitted=\"true\"><query>id:123</query></delete>"))
					.Repeat.Once()
					.Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithUniqueKey>(connection);
				ops.Delete(new SolrQuery("id:123"), true, true);
			});
		}

		[Test]
		[ExpectedException(typeof (NoUniqueKeyException))]
		public void DeleteDocumentWithoutUniqueKey_ShouldThrow() {
			var mocks = new MockRepository();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Delete(new TestDocumentWithoutUniqueKey());
			});
		}

		[Test]
		public void DeleteDocumentWithUniqueKey() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<delete><id>0</id></delete>")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithUniqueKey>(connection);
				ops.Delete(new TestDocumentWithUniqueKey());
			});
		}

		[Test]
		public void DeleteDocumentWithUniqueKeyWithParams() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<delete fromPending=\"true\" fromCommitted=\"false\"><id>0</id></delete>")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithUniqueKey>(connection);
				ops.Delete(new TestDocumentWithUniqueKey(), true, false);
			});
		}

		[Test]
		public void Optimize() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<optimize />")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Optimize();
			});
		}

		[Test]
		public void OptimizeWithOptions() {
			var mocks = new MockRepository();
			//ISolrDocument doc = mocks.CreateMock<ISolrDocument>();
			var connection = mocks.CreateMock<ISolrConnection>();
			With.Mocks(mocks).Expecting(delegate {
				Expect.Call(connection.Post("/update", "<optimize waitSearcher=\"true\" waitFlush=\"true\" />")).Repeat.Once().Return(null);
			}).Verify(delegate {
				var ops = new SolrServer<TestDocumentWithoutUniqueKey>(connection);
				ops.Optimize(true, true);
			});
		}

		[Test]
		public void QueryWithPagination() {
			const string qstring = "id:123";
			const int start = 10;
			const int rows = 20;

			var mocks = new MockRepository();
			var connection = mocks.CreateMock<ISolrConnection>();
			var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocumentWithUniqueKey>>();
			With.Mocks(mocks).Expecting(delegate {
				var query = new Dictionary<string, string>();
				query["q"] = qstring;
				query["start"] = start.ToString();
				query["rows"] = rows.ToString();
				Expect.Call(connection.Get("/select", query)).Repeat.Once().Return("");

				SetupResult.For(parser.Parse(null)).IgnoreArguments().Return(new SolrQueryResults<TestDocumentWithUniqueKey>());
			}).Verify(delegate {
				var solr = new SolrServer<TestDocumentWithUniqueKey>(connection) {ResultParser = parser};
				var r = solr.Query(new SolrQuery(qstring), start, rows);
			});
		}

		[Test]
		public void QueryWithSort() {
			const string qstring = "id:123";

			var mocks = new MockRepository();
			var connection = mocks.CreateMock<ISolrConnection>();
			var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocumentWithUniqueKey>>();
			With.Mocks(mocks).Expecting(delegate {
				IDictionary<string, string> query = new Dictionary<string, string>();
				query["q"] = qstring;
				query["sort"] = "id asc,name desc";
				Expect.Call(connection.Get("/select", query)).Repeat.Once().Return("");
				SetupResult.For(parser.Parse(null)).IgnoreArguments().Return(new SolrQueryResults<TestDocumentWithUniqueKey>());
			}).Verify(delegate {
				var solr = new SolrServer<TestDocumentWithUniqueKey>(connection) {ResultParser = parser};
				var r = solr.Query(new SolrQuery(qstring), new[] {new SortOrder("id", Order.ASC), new SortOrder("name", Order.DESC)});
			});
		}

		[Test]
		public void QueryWithSortAndPagination() {
			const string qstring = "id:123";
			const int start = 10;
			const int rows = 20;

			var mocks = new MockRepository();
			var connection = mocks.CreateMock<ISolrConnection>();
			var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocumentWithUniqueKey>>();
			With.Mocks(mocks).Expecting(delegate {
				var query = new Dictionary<string, string>();
				query["q"] = qstring;
				query["start"] = start.ToString();
				query["rows"] = rows.ToString();
				query["sort"] = "id asc,name desc";
				Expect.Call(connection.Get("/select", query)).Repeat.Once().Return("");

				SetupResult.For(parser.Parse(null)).IgnoreArguments().Return(new SolrQueryResults<TestDocumentWithUniqueKey>());
			}).Verify(delegate {
				var solr = new SolrServer<TestDocumentWithUniqueKey>(connection) {ResultParser = parser};
				var r = solr.Query(new SolrQuery(qstring), start, rows, new[] {new SortOrder("id", Order.ASC), new SortOrder("name", Order.DESC)});
			});
		}

		[Test]
		public void SearchResults_ShouldBeIterable() {
			var mocks = new MockRepository();
			var results = mocks.CreateMock<ISolrQueryResults<ISolrDocument>>();
			Assert.IsInstanceOfType(typeof (IEnumerable<ISolrDocument>), results);
		}
	}
}