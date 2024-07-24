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
    EmployeeId = 1234,
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

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
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

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(employee);
});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(ci => ci.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) => 
{
    ServiceTicket ticket = serviceTickets.FirstOrDefault(t => t.Id == id);
    serviceTickets.Remove(ticket);
    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
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

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/servicetickets/emergency", () =>
{
    List<ServiceTicket> incompleteEmergencies = serviceTickets.Where(st => st.DateCompleted == null && st.Emergency).ToList();
    if (incompleteEmergencies == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(incompleteEmergencies);
});

app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    if (unassignedTickets == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(unassignedTickets);
});

app.MapGet("/customers/archived", () =>
{
    DateTime oneYearAgo = DateTime.Now.AddYears(-1);
    List<Customer> archivedCustomers = customers.Where(c => !serviceTickets.Any(st => st.CustomerId == c.Id && st.DateCompleted.HasValue && st.DateCompleted.Value > oneYearAgo)).ToList();
    if (archivedCustomers.Count == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(archivedCustomers);
});

app.MapGet("/employees/available", () =>
{
    List<Employee> availableEmployees = employees.Where(e => !serviceTickets.Any(st => st.EmployeeId == e.Id && st.DateCompleted == null)).ToList();
    if (availableEmployees.Count == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(availableEmployees);
});

app.Run();

