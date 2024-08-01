using System.Security.Principal;
using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer>()
{
       new Customer()
       {
           Id = 1,
           Name = "Drew",
           Address = "1234 Awesome Lane, Nashville, TN 37208"
       },

       new Customer()
       {
           Id = 2,
           Name = "Lola",
           Address = "1234 Even More Drive, Nashville, TN 37208"
       },

       new Customer()
       {
           Id = 3,
           Name = "Sarah",
           Address = "400 Kentucky Drive, Radcliff, KY 40175"
       }
};
List<Employee> employees = new List<Employee>()
{
    new Employee()
    {
        Id = 1234,
        Name = "John Doe",
        Specialty = "Vendor Portal"
    },

    new Employee()
    {
        Id = 4567,
        Name = "Jane McKinnon",
        Specialty = "Outlook"
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>()
{
    new ServiceTicket()
    {
    Id = 11,
    CustomerId = 1,
    EmployeeId = 1234,
    Description = "This is a test description.",
    Emergency = false,
    DateCompleted = new DateTime(2021, 7, 19)
    },

    new ServiceTicket()
    {
    Id = 12,
    CustomerId = 2,
    EmployeeId = null,
    Description = "Houston, we have a problem.",
    Emergency = true,
    DateCompleted = null
    },

    new ServiceTicket()
    {
    Id = 13,
    CustomerId = 2,
    EmployeeId = null,
    Description = "We've have a big issue here.",
    Emergency = false,
    DateCompleted = null
    },

    new ServiceTicket()
    {
    Id = 14,
    CustomerId = 3,
    EmployeeId = 1234,
    Description = "Help, this is an emergency.",
    Emergency = true,
    DateCompleted = null
    },

    new ServiceTicket()
    {
    Id = 15,
    CustomerId = 2,
    EmployeeId = 4567,
    Description = "Actually, everything is good.",
    Emergency = false,
    DateCompleted = new DateTime(2024, 7, 20)
    },

    new ServiceTicket()
    {
    Id = 16,
    CustomerId = 2,
    EmployeeId = 4567,
    Description = "Date Test 1",
    Emergency = false,
    DateCompleted = new DateTime(2024, 6, 19)
    },

    new ServiceTicket()
    {
    Id = 17,
    CustomerId = 2,
    EmployeeId = 4567,
    Description = "Date Test 2",
    Emergency = false,
    DateCompleted = new DateTime(2024, 7, 21)
    },

    new ServiceTicket()
    {
    Id = 18,
    CustomerId = 2,
    EmployeeId = 4567,
    Description = "Date Test 2",
    Emergency = false,
    DateCompleted = null
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/api/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/api/employees", () =>
{
    return employees;
});

app.MapGet("/api/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(employee);
});

app.MapGet("/api/customers", () =>
{
    return customers;
});

app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(ci => ci.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/api/servicetickets/{id}", (int id) => 
{
    ServiceTicket ticket = serviceTickets.FirstOrDefault(t => t.Id == id);
    serviceTickets.Remove(ticket);
    return Results.NoContent();
});

app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/api/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

// Emergency Tickets
app.MapGet("/api/servicetickets/emergency", () =>
{
    List<ServiceTicket> incompleteEmergencies = serviceTickets.Where(st => st.DateCompleted == null && st.Emergency).ToList();
    if (incompleteEmergencies == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(incompleteEmergencies);
});

// Unassigned Tickets
app.MapGet("/api/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    if (unassignedTickets == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(unassignedTickets);
});

// Inactive Customers
app.MapGet("/api/customers/archived", () =>
{
    DateTime oneYearAgo = DateTime.Now.AddYears(-1);
    List<Customer> archivedCustomers = customers.Where(c => !serviceTickets.Any(st => st.CustomerId == c.Id && st.DateCompleted.HasValue && st.DateCompleted.Value > oneYearAgo)).ToList();
    if (archivedCustomers.Count == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(archivedCustomers);
});

// Available Employees
app.MapGet("/api/employees/available", () =>
{
    List<Employee> availableEmployees = employees.Where(e => !serviceTickets.Any(st => st.EmployeeId == e.Id && st.DateCompleted == null)).ToList();
    if (availableEmployees.Count == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(availableEmployees);
});

// Employee's Customers
app.MapGet("/api/employees/{id}/customers", (int id) =>
{
    var customerIds = serviceTickets.Where(st => st.EmployeeId == id).Select(st => st.CustomerId).Distinct().ToList();

    List<Customer> employeesCustomers = customers.Where(c => customerIds.Contains(c.Id)).ToList();

    if (employeesCustomers.Count == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(employeesCustomers);
});

// Employee of the Month
app.MapGet("/api/monthlyemployee", () =>
{
    var employeeCompletedTicketCounts = serviceTickets
    .Where(st => st.EmployeeId != null && st.DateCompleted != null)
    .GroupBy(st => st.EmployeeId)
    .Select(group => new
    {
        EmployeeId = group.Key,
        TicketCount = group.Count()
    })
    .OrderByDescending(x => x.TicketCount)
    .FirstOrDefault();

    if (employeeCompletedTicketCounts == null)
    {
        return Results.NotFound();
    }

    var highestTicketCountEmployee = employees.FirstOrDefault(e => e.Id == employeeCompletedTicketCounts.EmployeeId);

    return Results.Ok(highestTicketCountEmployee);
});

// Past Ticket Review
app.MapGet("/api/completedtickets", () =>
{
   return serviceTickets.Where(st => st.DateCompleted != null).OrderBy(st => st.DateCompleted);
});

// Prioritized Tickets
app.MapGet("/api/prioritytickets", () =>
{
    List<ServiceTicket> incompleteTickets = serviceTickets.Where(st => st.DateCompleted == null).ToList();
    List<ServiceTicket> sortedTickets = incompleteTickets.OrderByDescending(st => st.Emergency).ThenByDescending(st => st.EmployeeId).ToList();
    return Results.Ok(sortedTickets);
});


app.Run();

