using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class EmployeeResponse
    {
        public string api { get; set; }
        public EmployeeData data { get; set; }
    }

    public class EmployeeData
    {
        public EmployeeResult result { get; set; }
    }

    public class EmployeeResult
    {
        public List<ChildDepartment> childDepartments { get; set; }
    }

    public class ChildDepartment
    {
        public long id { get; set; }
        public string name { get; set; }
        public long parentId { get; set; }
        public List<Employee> employeeList { get; set; }
        public List<ChildDepartment> childDepartments { get; set; }
    }

    public class Employee
    {
        public long departmentId { get; set; }
        public string departmentName { get; set; }
        public string headUrl { get; set; }
        public string name { get; set; }
        public string nickName { get; set; }
        public long subUserId { get; set; }
        public long userId { get; set; }
    }
}
