using I72_Backend.Entities;
using I72_Backend.Entities.Enums;
using I72_Backend.Interfaces;
using Mysqlx.Datatypes;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;

namespace I72_Backend.Services;

public class ManagementService : IManagementService
{
    private readonly IManagementRepository _repository;
    private readonly ILogger<ManagementService> _logger;

    public ManagementService(IManagementRepository repository, ILogger<ManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void CreateTables(CreateListTablesDto dto)
    {
        var sqlScript = @"";
        foreach (var table in dto.Menu)
        {
            var columnTypes = string.Join(", ", table.ColumnDefinitions.Select(column =>
                $"{column.Name} {column.Type}{(column.Key ? " PRIMARY KEY" : "")}"
            ));
            var createScript = $"CREATE TABLE IF NOT EXISTS `{table.TableName}`({columnTypes}, updated_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP);";
            sqlScript += createScript;
        }

        _logger.LogInformation($"Create query: {sqlScript}");
        _repository.ExecuteCreateScript(sqlScript);
    }

    public List<Dictionary<string, object>> GetBarChartData(String table, String columnX, String columnY,
        AggregationType aggregationType)
    {
        String query = $@"SELECT {columnX}, {aggregationType}({columnY}) FROM {table} GROUP BY {columnX}";
        _logger.LogInformation($"Get bar chart query: {query}");
        return _repository.ExecuteQuery(query);
    }

    public String PerformInsert(String table, Dictionary<String, String> values)
    {
        // Generate the columns part of the query by joining the keys of the dictionary
        String columns = string.Join(", ", values.Keys);
        // Generate the values part of the query by joining the values of the dictionary, and ensuring they are properly quoted
        String valuesString = string.Join(", ", values.Values.Select(v => $"'{v}'"));

        // Build the full SQL query
        String query = $@"INSERT INTO `{table}` ({columns}) VALUES ({valuesString})";
        _logger.LogInformation($"Insert query: {query}");
        int insertedRows = _repository.ExecuteCreateScript(query);

        return $"{insertedRows} rows have been inserted";
    }

    public String PerformBatchUpdate(String table, Dictionary<String, String?> whereCondition,
        Dictionary<String, String?> updateFields)
    {
        String whereConditionString =
            string.Join("AND", whereCondition.Keys.Select(k => $"`{k}` = '{whereCondition[k]}'"));
        String setStatementString = string.Join(",", updateFields.Keys.Select(k => $"`{k}` = '{updateFields[k]}'"));
        String query = $@"UPDATE `{table}` SET {setStatementString} WHERE {whereConditionString};";
        _logger.LogInformation($"Batch update query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} rows have been updated";
    }

    public String PerformDeleteById(String table, String column, String id)
    {
        String query = $@"DELETE FROM {table} WHERE `{column}` = '{id}'";
        _logger.LogInformation($"Delete query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} row has been deleted";
    }

    public PageableResult PerformRead(String table, PaginationParams pageParams,
        Dictionary<String, String?> conditions)
    {
        List<Dictionary<String, object?>> res = _repository.GetRowsByTable(table, conditions, pageParams);
        _logger.LogInformation($"Read query get: {res.Count} records");
        var pageableRes = new PageableResult();
        pageableRes.Page = pageParams.Page;
        pageableRes.PageSize = pageParams.PageSize;
        pageableRes.Rows = res;
        pageableRes.TotalPage = _repository.GetTotalPages(table, conditions, pageParams);
        return pageableRes;
    }

    public String PerformBatchDelete(String table, Dictionary<String, String?> whereConditions)
    {
        String whereConditionString = string.Join(" AND ",
            whereConditions.Keys.Select(k =>  $"`{k}` LIKE '{whereConditions[k]}'"));
        String query = 
            $@"DELETE FROM `{table}` WHERE {whereConditionString}";
        _logger.LogInformation($"Delete query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} row has been deleted";
    }

//    public FileStreamResult GeneratePdfReport(string table, string x, string y, string aggregationFunction)
// {
//     try
//     {
//         // Define the path to a temporary file
//         var tempFilePath = Path.Combine(Path.GetTempPath(), $"DashboardReport_{Guid.NewGuid()}.pdf");

//         // Write the PDF content to the temp file
//         using (var writer = new PdfWriter(tempFilePath))
//         {
//             var pdf = new PdfDocument(writer);
//             var document = new Document(pdf);

//             // Add content to the PDF (Title and Date)
//             document.Add(new Paragraph("Business Dashboard Report"));
//             document.Add(new Paragraph($"Generated on: {DateTime.Now}"));

//             // Determine the aggregation type based on user input
//             AggregationType aggregationType = aggregationFunction switch
//             {
//                 "Sum" => AggregationType.SUM,
//                 "Count" => AggregationType.COUNT,
//                 "AVG" => AggregationType.AVG,
//                 "Max" => AggregationType.MAX,
//                 "Min" => AggregationType.MIN,
//                 _ => AggregationType.SUM // Default to SUM if not provided
//             };

//             // Fetching data from the dashboard based on dynamic inputs
//             var chartData = GetAggregateChartData(table, x, y, aggregationType);

//             // **Check if chartData is null or empty**
//             if (chartData == null || !chartData.Any())
//             {
//                 _logger.LogError("Chart data is null or empty. Cannot generate PDF.");
//                 throw new Exception("No data available to generate PDF report.");
//             }

//             // Add fetched data as a table to the PDF
//             Table pdfTable = new Table(3); // 3 columns: X, Y, Aggregated Value
//             pdfTable.AddHeaderCell("X Value");
//             pdfTable.AddHeaderCell("Y Value");
//             pdfTable.AddHeaderCell("Aggregated Value");

//             // Populate the table with chart data, adding null checks
//             foreach (var row in chartData)
//             {
//                 var xValue = row.ContainsKey(x) && row[x] != null ? row[x]?.ToString() : "N/A";
//                 var yValue = row.ContainsKey(y) && row[y] != null ? row[y]?.ToString() : "N/A";
//                 var aggregatedValue = row.ContainsKey("AggregatedValue") && row["AggregatedValue"] != null
//                     ? row["AggregatedValue"]?.ToString()
//                     : "N/A";

//                 pdfTable.AddCell(new Cell().Add(new Paragraph(xValue)));
//                 pdfTable.AddCell(new Cell().Add(new Paragraph(yValue)));
//                 pdfTable.AddCell(new Cell().Add(new Paragraph(aggregatedValue)));
//             }

//             document.Add(pdfTable); // Add table to the document
//             document.Close();  // Close the document to finalize it
//         }

//         // Return the PDF file as a FileStreamResult for download
//         var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
//         return new FileStreamResult(stream, "application/pdf")
//         {
//             FileDownloadName = "dashboard-report.pdf"
//         };
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Error while generating PDF report");
//         throw new Exception("Error while generating PDF", ex);
//     }
// }


public FileStreamResult GeneratePdfReport(string table, string x, string y, string aggregationFunction)
        {
            try
            {
                // Define the path to a temporary file
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"DashboardReport_{Guid.NewGuid()}.pdf");

                // Write the PDF content to the temp file
                using (var writer = new PdfWriter(tempFilePath))
                {
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    // Add content to the PDF (Title and Date)
                    document.Add(new Paragraph("Business Dashboard Report"));
                    document.Add(new Paragraph($"Generated on: {DateTime.Now}"));

                    // Determine the aggregation type
                    AggregationType aggregationType = aggregationFunction switch
                    {
                        "Sum" => AggregationType.SUM,
                        "Count" => AggregationType.COUNT,
                        "AVG" => AggregationType.AVG,
                        "Max" => AggregationType.MAX,
                        "Min" => AggregationType.MIN,
                        _ => AggregationType.SUM // Default to SUM if not provided
                    };

                    // Fetch data for different charts
                    var barChartData = GetAggregateChartData(table, x, y, aggregationType); // For Bar Chart
                    var lineChartData = GetAggregateChartData(table, x, y, aggregationType); // For Line Chart
                    var pieChartData = GetAggregateChartData(table, x, y, AggregationType.SUM); // For Pie Chart

                    // Add Bar Chart Section
                    AddChartSection(document, "Bar Chart Data", barChartData, x, y);

                    // Add Line Chart Section
                    AddChartSection(document, "Line Chart Data", lineChartData, x, y);

                    // Add Pie Chart Section
                    AddChartSection(document, "Pie Chart Data", pieChartData, x, y);

                    document.Close();
                }

                // Return the PDF file as a FileStreamResult for download
                var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = "dashboard-report.pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating PDF report");
                throw new Exception("Error while generating PDF", ex);
            }
        }
// Helper function to add chart data section to PDF
        private void AddChartSection(Document document, string sectionTitle, List<Dictionary<string, object>> chartData, string x, string y)
        {
            if (chartData == null || !chartData.Any())
            {
                document.Add(new Paragraph($"No data available for {sectionTitle}").SetBold());
                return;
            }

            // Add a heading for the chart type
            document.Add(new Paragraph(sectionTitle).SetBold());

            // Create the table for X, Y, and Aggregated Value columns
            Table pdfTable = new Table(3); // 3 columns: X Value, Y Value, Aggregated Value
            pdfTable.AddHeaderCell("X Value");
            pdfTable.AddHeaderCell("Y Value");
            pdfTable.AddHeaderCell("Aggregated Value");

            // Populate the table with chart data
            foreach (var row in chartData)
            {
                string xValue = row.ContainsKey(x) ? row[x]?.ToString() ?? "N/A" : $"Key '{x}' not found";
                string yValue = row.ContainsKey(y) ? row[y]?.ToString() ?? "N/A" : $"Key '{y}' not found";
                string aggregatedValue = row.ContainsKey("AggregatedValue") ? row["AggregatedValue"]?.ToString() ?? "N/A" : "AggregatedValue not found";

                pdfTable.AddCell(new Cell().Add(new Paragraph(xValue)));
                pdfTable.AddCell(new Cell().Add(new Paragraph(yValue)));
                pdfTable.AddCell(new Cell().Add(new Paragraph(aggregatedValue)));
            }

            // Add the table to the document
            document.Add(pdfTable);
        }

        public List<Dictionary<string, object>> GetAggregateChartData(String table, String columnX, String columnY, AggregationType aggregationType)
        {
            // Construct the SQL query dynamically
            String query = $@"SELECT {columnX}, {aggregationType}({columnY}) AS AggregatedValue FROM {table} GROUP BY {columnX}";
            _logger.LogInformation($"Get aggregate chart query: {query}");

            // Execute the query and return the results
            return _repository.ExecuteQuery(query);
        }
    }



// public FileStreamResult GeneratePdfReport(string table, string x, string y, string aggregationFunction)
// {
//     try
//     {
//         // Define the path to a temporary file
//         var tempFilePath = Path.Combine(Path.GetTempPath(), $"DashboardReport_{Guid.NewGuid()}.pdf");

//         // Write the PDF content to the temp file
//         using (var writer = new PdfWriter(tempFilePath))
//         {
//             var pdf = new PdfDocument(writer);
//             var document = new Document(pdf);

//             // Add content to the PDF (Title and Date)
//             document.Add(new Paragraph("Business Dashboard Report"));
//             document.Add(new Paragraph($"Generated on: {DateTime.Now}"));

//             // Determine the aggregation type
//             AggregationType aggregationType = aggregationFunction switch
//             {
//                 "Sum" => AggregationType.SUM,
//                 "Count" => AggregationType.COUNT,
//                 "AVG" => AggregationType.AVG,
//                 "Max" => AggregationType.MAX,
//                 "Min" => AggregationType.MIN,
//                 _ => AggregationType.SUM // Default to SUM if not provided
//             };

//             // Fetch data from the dashboard for different chart types
//             var barChartData = GetAggregateChartData(table, x, y, aggregationType); // For Bar Chart
//             var lineChartData = GetAggregateChartData(table, x, y, aggregationType); // For Line Chart
//             var pieChartData = GetAggregateChartData(table, x, y, AggregationType.SUM); // For Pie Chart (Sum-based aggregation)

//             // Add Bar Chart Section
//             AddChartSection(document, "Bar Chart Data", barChartData, x, y);

//             // Add Line Chart Section
//             AddChartSection(document, "Line Chart Data", lineChartData, x, y);

//             // Add Pie Chart Section
//             AddChartSection(document, "Pie Chart Data", pieChartData, x, y);

//             document.Close();
//         }

//         // Return the PDF file as a FileStreamResult for download
//         var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
//         return new FileStreamResult(stream, "application/pdf")
//         {
//             FileDownloadName = "dashboard-report.pdf"
//         };
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Error while generating PDF report");
//         throw new Exception("Error while generating PDF", ex);
//     }
// }

// // Helper function to add chart data section to PDF
// private void AddChartSection(Document document, string sectionTitle, List<Dictionary<string, object>> chartData, string x, string y)
// {
//     if (chartData == null || !chartData.Any())
//     {
//         document.Add(new Paragraph($"No data available for {sectionTitle}").SetBold());
//         return;
//     }

//     // Add a heading for the chart type
//     document.Add(new Paragraph(sectionTitle).SetBold());

//     // Create the table for X, Y, and Aggregated Value columns
//     Table pdfTable = new Table(3); // 3 columns: X Value, Y Value, Aggregated Value
//     pdfTable.AddHeaderCell("X Value");
//     pdfTable.AddHeaderCell("Y Value");
//     pdfTable.AddHeaderCell("Aggregated Value");

//     // Populate the table with chart data
//     foreach (var row in chartData)
//     {
//         string xValue = row.ContainsKey(x) ? row[x]?.ToString() ?? "N/A" : $"Key '{x}' not found";
//         string yValue = row.ContainsKey(y) ? row[y]?.ToString() ?? "N/A" : $"Key '{y}' not found";
//         string aggregatedValue = row.ContainsKey("AggregatedValue") ? row["AggregatedValue"]?.ToString() ?? "N/A" : "AggregatedValue not found";

//         pdfTable.AddCell(new Cell().Add(new Paragraph(xValue)));
//         pdfTable.AddCell(new Cell().Add(new Paragraph(yValue)));
//         pdfTable.AddCell(new Cell().Add(new Paragraph(aggregatedValue)));
//     }

//     // Add the table to the document
//     document.Add(pdfTable);
// }


//        public List<Dictionary<string, object>> GetAggregateChartData(String table, String columnX, String columnY, AggregationType aggregationType)
// {
//     // Construct the SQL query dynamically
//     String query = $@"SELECT {columnX}, {aggregationType}({columnY}) AS AggregatedValue FROM {table} GROUP BY {columnX}";
//     _logger.LogInformation($"Get aggregate chart query: {query}");

//     // Execute the query and return the results
//     return _repository.ExecuteQuery(query);
// }


         
