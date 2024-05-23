# Aiirh.Audit

This library provides functionality of tracking changes between two objects of the same data class, based on comparing so-called `Revisions`, that have to be created and stored every time when trackable data change is done

## Installing package

Install package using NuGet Package Manager
```
Install-Package Aiirh.Audit
```

or using .NET CLI

```
dotnet add package Aiirh.Audit
```

## How to use
### Prepare your data class
To make class properties to track changes, mark them with `Auditable` attribute. Only properties with this attribute are trackable.

```
using Aiirh.Audit;

public class Product
{
    [Auditable(DisplayName = "Product price", PropertyName = "Price")]
    public decimal Price { get; set; }

    public string Name { get; set; } // Changes of this property value will not be tracked
}
```

`DisplayName` parameter is used to specify, what label to show (e.g. in UI) as a name/label of changed property.

`PropertyName` parameter is an override for the real property name. It can be useful in situations, when property in the class is renamed, but you want to support previous revisions that have been created before renaming. Comparing values is done either by `PropertyName` attribute if provided, or by ther real property name how it is named in the class.

### Create revisions

Every time when change is done (e.g. `Price` is updated), you need to create a `Revision`, providing a date when revision was created, author of the change, and a custom comment

```
// Find your product and update the price
var product = await myProductRepository.FindMyProduct();
product.Price = 123m;

// Create revision for updated product
var revision = product.CreateRevision(DateTime.UtcNow, "John Smith", "Price has been updated manually from admin page");
```

Now you need to store this revision in a way to be able to find revisions for exact this product later.

**Note!** Since at least two revisions are needed to detect a change, there is a need to create *initial* revision on object creation. Otherwise the first change will not be shown.

### Build audit log

Read all revision related to the same object

```
// Find the product
var product = await myProductRepository.FindMyProduct();

// Read related revisions
var revisions = await myRevisionRepository.GetAllRevisionsByProductId(product.Id);

// Build audit log
var auditLogs = revisions.ToAuditLog();
```

`auditLogs` is a collection `IEnumerable<IAuditLog>`. One `IAuditLog` represents one change between two revisions. 
`IAuditLog` contains a collection `IEnumerable<IAuditLogEntry> Changes`. One `IAuditLogEntry` represents one change of one property marked with `Auditable` attribute.

### Result

Use `auditLogs` data to display the `product` change history in any manner you need.

**Note!** This package is not supposed to handle big volums of data, so the method `.ToAuditLog()` may come across poor performance working with hundreds of revisions, so it has an optional paratemeter `int? depth = null` to limit output and returning only `depth` latest changes. Also you can limit an amount of provided revisions on your side.
