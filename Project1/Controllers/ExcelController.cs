using Microsoft.AspNetCore.Mvc;
/*using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;*/

namespace Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {

        /*        private string readExcel()
                {
                    string filePath = "C:\\Users\\ilyas\\Desktop\\demo.xlsx";
                    _Excel.Application excel = new _Excel.Application();

                    Workbook wb;
                    Worksheet ws;

                    wb = excel.Workbooks.Open(filePath);

                    ws = wb.Worksheets[1];

                    _Excel.Range cell = ws.Range["A1"];

                    string cellValue = cell.Value;

                    return cellValue;
                }

                [HttpGet]
                public IActionResult GetFirstCellValue()
                {
                    string value = readExcel();
                    return Ok(value);
                }*/

        [HttpGet]
        public IActionResult Time()
        {
            DateTime dateTime = DateTime.Now;
            return Ok(dateTime);
        }
    }
}
