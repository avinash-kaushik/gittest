using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using EmployeeCollection.WebAPI.Data;
using EmployeeCollection.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace EmployeeCollection.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeCollectionService _empService;
        public EmployeeController(IEmployeeCollectionService empService)
        {
            _empService = empService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonResult AddEmployee(Employee employee)
        {
            try
            {
                if (employee.Id == 0)
                {
                    employee.Id = _empService.GetLastEmpployeeId() + 1;
                }

                _empService.AddEmployee(employee);

                return new JsonResult(employee)
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch
            {
                return new JsonResult(employee)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }


        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<JsonResult> GetAllEmployeeWithDeptFilter()
        {
            try
            {
                string department = string.Empty;
                string age = string.Empty;
                string location = string.Empty;
                department = !string.IsNullOrEmpty(HttpContext.Request.Query["department"].ToString())
                                                    ? HttpContext.Request.Query["department"].ToString() : "";

                age = !string.IsNullOrEmpty(HttpContext.Request.Query["Age"].ToString()) ?
                        HttpContext.Request.Query["Age"].ToString() : "";


                location = !string.IsNullOrEmpty(HttpContext.Request.Query["location"].ToString()) ?
                        HttpContext.Request.Query["location"].ToString() : "";

                Filters datafilter = new Filters();


                if (!string.IsNullOrEmpty(department))
                {
                    datafilter.Department = department.ToUpper().Split(',').ToArray();
                }

                if (!string.IsNullOrEmpty(location))
                {
                    datafilter.Location = new List<string> { location.ToUpper() }.ToArray();
                }

                if (!string.IsNullOrEmpty(age))
                {
                    datafilter.Age = new List<int> { int.Parse(age) }.ToArray();
                }

                var records = await _empService.GetEmployees(null, datafilter);

                return new JsonResult(records)
                {
                    StatusCode = (records.Count > 0) ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
                };
            }
            catch
            {
                return new JsonResult("Error")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<JsonResult> GetEmployeeWithId(int id)
        {
            try
            {
                int[] ids = new int[] { id };
                var records = await _empService.GetEmployees(ids, null);
                return new JsonResult(records)
                {
                    StatusCode = (records.Count > 0) ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
                };
            }
            catch
            {
                return new JsonResult(id)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<JsonResult> DeleteEmployeeWithId(int id)
        {
            try
            {
                int[] ids = new int[] { id };
                var records = await _empService.GetEmployees(ids, null);

                if (records.Count == 0)
                {
                    return new JsonResult(id)
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                else
                {
                    Employee firstrec = records.Where(x => x.Id == id).FirstOrDefault();
                    var result = await _empService.DeleteEmployee(firstrec);
                    return new JsonResult(id)
                    {
                        StatusCode = StatusCodes.Status204NoContent
                    };
                }
            }
            catch
            {
                return new JsonResult(id)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }
    }

}
