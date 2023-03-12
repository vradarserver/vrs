using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Test.Framework
{
    /// <summary>
    /// A kludge that lets me have inline data tests like DataRow without going
    /// through the pain of converting to MSTest.TestFramework. This also lets
    /// me use types that cannot be declared at compile time (like dates).
    /// </summary>
    /// <remarks>
    /// In the .NET Core world it turns out that these can still be useful for
    /// tests that involve non-literal values, e.g. dates, decimals etc. You
    /// can't put those into attributes, but this thing will let you do anything.
    /// </remarks>
    public class InlineDataTest
    {
        /// <summary>
        /// Describes an exception thrown during a test and the context in which it was thrown.
        /// </summary>
        private class ExceptionContext
        {
            public string Method { get; set; }

            public int TestLineNumber { get; set; }

            public Exception Exception { get; set; }
        }

        /// <summary>
        /// A map of all failed rows indexed by zero-based row number.
        /// </summary>
        private readonly Dictionary<int, ExceptionContext> _FailedRows = new Dictionary<int, ExceptionContext>();

        /// <summary>
        /// The test initialise method that will be called before each test.
        /// </summary>
        private MethodInfo _TestInitialize;

        /// <summary>
        /// The test cleanup method that will be called to clean up after the last invocation.
        /// </summary>
        private MethodInfo _TestCleanup;

        /// <summary>
        /// Gets the test class that has the test initialise and test cleanup methods that this will call.
        /// </summary>
        public object TestClass { get; }

        /// <summary>
        /// Gets the number of rows tested so far.
        /// </summary>
        public int CountRows { get; private set; }

        /// <summary>
        /// Gets the offset to add to zero-based row numbers when reporting errors. If your row source has
        /// 1-based row numbers then set this to 1, if it has 1-based rows and a header row then set to 2,
        /// if it's 0-based then leave at the default of 0.
        /// </summary>
        public int RowOffsetForErrorReports { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="testClass"></param>
        /// <param name="rowOffsetForErrorReports"></param>
        public InlineDataTest(object testClass, int rowOffsetForErrorReports = 0)
        {
            TestClass = testClass;
            FindTestInitialiseAndTestCleanup();
            RowOffsetForErrorReports = rowOffsetForErrorReports;
        }

        private void FindTestInitialiseAndTestCleanup()
        {
            if(TestClass != null) {
                _TestInitialize = TestClass
                    .GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(r => r.GetCustomAttribute<TestInitializeAttribute>(inherit: true) != null)
                    .FirstOrDefault();

                _TestCleanup = TestClass
                    .GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(r => r.GetCustomAttribute<TestCleanupAttribute>(inherit: true) != null)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Iterates through a collection of rows and calls the test method against each one. Any failures are reported
        /// once all rows have been tested.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="test"></param>
        public void TestAndAssert<T>(IEnumerable<T> rows, Action<T> test)
        {
            foreach(var row in rows) {
                ExecuteTest(() => test(row));
            }
            AssertAllWorked();
        }

        /// <summary>
        /// Executes the <paramref name="test"/> action and records the failure, if any. Increments <see cref="CountRows"/>
        /// before returning.
        /// </summary>
        /// <param name="test"></param>
        public void ExecuteTest(Action test)
        {
            try {
                // We want cleanup and then init, and both *before* the test, because at this point Initialise will
                // have run. We want to clean up from that and then run a brand new initialise and leave it in an
                // initialised state for the normal TestClass cleanup call.
                _TestCleanup?.Invoke(TestClass, null);
                _TestInitialize?.Invoke(TestClass, null);

                test();
            } catch(Exception ex) {
                var context = new ExceptionContext() {
                    Exception = ex,
                };
                _FailedRows[CountRows] = context;

                try {
                    var stackTrace = new StackTrace(ex, true);
                    foreach(var frame in stackTrace.GetFrames()) {
                        if(frame.HasMethod() && frame.HasSource()) {
                            context.Method =         frame.GetMethod()?.Name ?? "";
                            context.TestLineNumber = frame?.GetFileLineNumber() ?? 0;
                            break;
                        }
                    }
                } catch {
                    ;
                }
            }

            ++CountRows;
        }

        /// <summary>
        /// Throws a Fail assertion if any tests run by <see cref="ExecuteTest"/> threw an exception.
        /// </summary>
        public void AssertAllWorked()
        {
            if(_FailedRows.Count > 0) {
                var message = new StringBuilder();

                message.AppendLine();
                message.AppendLine($"{_FailedRows.Count:N0} / {CountRows:N0} test row{(CountRows == 1 ? "" : "s")} failed.");

                message.AppendLine();
                message.AppendLine($"SUMMARY (row numbers are 0-based)");
                message.AppendLine($"=================================");
                foreach(var kvp in _FailedRows.OrderBy(r => r.Key)) {
                    message.AppendLine($"Row {kvp.Key + RowOffsetForErrorReports} @{kvp.Value.TestLineNumber}: {kvp.Value.Exception.Message}");
                }

                message.AppendLine();
                message.AppendLine($"DETAIL");
                message.AppendLine($"======");
                var firstDetail = true;
                foreach(var kvp in _FailedRows.OrderBy(r => r.Key)) {
                    if(firstDetail) {
                        firstDetail = false;
                    } else {
                        message.AppendLine();
                    }
                    if(kvp.Value.TestLineNumber > 0) {
                        message.AppendLine($"Row {kvp.Key + RowOffsetForErrorReports} failed at line {kvp.Value.TestLineNumber} in {kvp.Value.Method}:{Environment.NewLine}{kvp.Value.Exception}");
                    } else {
                        message.AppendLine($"Row {kvp.Key + RowOffsetForErrorReports} failed: {kvp.Value.Exception}");
                    }
                }

                Assert.Fail(message.ToString());
            }
        }
    }
}
