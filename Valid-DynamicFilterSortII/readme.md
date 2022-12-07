
# Dynamic Filter Sort v2  
Dynamic Filter Sort v2 (DFS2) has been redesigned to be extendable and overridable.  It comes packaged   
with a default syntax parser and default implementation to enable support for `System.Linq.Dynamic.Core` out of the box.    
  
The default implementation of the syntax parser and integration with `System.Linq.Dynamic.Core` is  
mostly feature complete with DFS1.  
  
Included with this version is a new `Json` class to hold unstructured data.  This class is near-identical to the `Json` class used in other Valid solutions (and should replace that class when upgrading from DFS1 to DFS2).  
  
## Using Dynamic Filter Sort v2  in a project
  
### Registration  
  
In an ASP.NET Core project, simply register DFS2 in `Startup.cs` in the `ConfigureServices` method using   
```c#
services.UseDynamicFilterSort();  
```  
or  
```c#
services.UseDynamicFilterSort(dfsConfig => {  
// registers a custom ISyntaxParser 
    dfsConfig.AddSyntaxParser<TSyntaxParser>();  
// registers a custom ISyntaxParser<TParameter> and optionally sets it as    
// the default parser for a type implementing IParameter  
    dfsConfig.AddSyntaxParser<TSyntaxParser, TParameter>(asDefault: true);  
// registers a data type module and its settings 
    dfsConfig.AddDataTypeModule<TModule>();}  
```  
In any other project, register DFS2 in the main method replacing `services` with `this`.  
  
#### Accessing DFS Service  
In a project using Dependency Injection (DI), simply DI `IDynamicFilterSortService` into your services.  In instances where  
DI is not possible, DFS2 comes with a `ServiceLocator` baked in.  ```ServiceLocator.GetService<IDynamicFilterSortService>```  
  
#### Making a request  
Instantiate an instance of `FilterSortRequest<TEntity>` where `TEntity` is the type of entity (or DTO) to query against.  
```c#
var request = new FilterSortRequest<MyEntity>  
{  
// optional, default of 10 (ignored when getting count) 
    Count = 10, 
// optional, default of 0 (ignored when getting count) 
    Offset = 0, 
// optional, no filter required 
    Filter = "LastName=Smith", 
// optional, no sort required 
    Sort = "FirstName=Asc", 
// optional, defaults to DefaultSyntaxParser 
    SyntaxParser = typeof(DefaultSyntaxParser),    
// required, a new instance of a type that implements BaseDataAccessConfiguration<TEntity>  
// this is used to allow the data accessor to retrieve the data.  For DynamicLinq there's 
// just one property where the dataset is passed in as an IQueryable 
    DataAccessConfiguration = new DynamicLinqDataAccessConfiguration<MyEntity> 
    { 
        DataSource = MyEntityList.AsQueryable() 
    }
}  
```  
#### Get Count  
This method will apply filters to the records and return the total number of records that meet the criteria.  
It will not honor the count/offset parameters.  
  
```c#
int total = _dynamicFilterSort.GetCount(request);  
```  
  
#### Get Enumerable  
This method will apply filters and sorts to the records and will return an enumerable of the results.  
It will honor the count/offset parameters.  
```c#
IEnumerable<MyEntity> result = _dynamicFilterSort.GetEnumerable(request);  
```  
  
#### Get Pagination Model  
This method will apply filters and sorts to the records and will return a Pagination Model of the results,  
containing the total, count, offset, and List<TEntity>  
```c#
IPaginationModel<MyEntity> result = _dynamicFilterSort.GetPaginationModel(request);  
```  
  
## Extending DFS2  
Dynamic Filter Sort 2 allows for Syntax Parsers to be extended / replaced and allows expanding new ways to access data.   
By default, DFS2 includes DynamicLinq data access which can work on in-memory collections as IQueryable, and even with  
EFCore as DbSets implement IQueryable.  
  
### Syntax Parsers  
DFS2 allows syntax parsers to be replaced in part, or in whole, and substituted in each request   
- `ISyntaxParser` -- a syntax parser that returns IParameters (default will find an appropriate ISyntaxParser<T>   
    for the parameter's type if available).  
- `ISyntaxParser<TParameter>` -- a syntax parser that returns a specific parameter type that implements IParameter.  
  
### Data Interfaces  
DFS2 is not just limited to DynamicLinq, new data interfaces can be added by implementing  
- `BaseDataInterfaceRegistration` -- an abstract class used to hold registration info for the data interface; e.g. DynamicLinq.cs  
- `BaseDataAccessConfiguration` -- an abstract class used to hold configuration properties for how to access  
    the data via the IDataAccessor; e.g. DynamicLinqDataAccessConfiguration.cs  
- `BaseDataSyntaxModel` -- an abstract class used to take parameters created from   
    ISyntaxParser and make them in usable format for this data interface via an IDataSyntaxBuilder; e.g. DynamicLinqDataSyntaxModel.cs  
- `IDataSyntaxBuilder` -- Translates parameters into usable format for the data interface; e.g. DynamicLinqDataSyntaxBuilder.cs  
- `IDataSyntaxBuilder<in TParameter, out TDataSyntax>` -- Translates parameters into usable format for   
    the data interface and into a specific DataSyntaxModel; e.g. DynamicLinqFilterDataSyntaxBuilder.cs, DynamicLinqSortDataSyntaxBuilder.cs  
 - `IDataAccessor` -- an abstract class used to retrieve data, count, or pagination model for a data interface type; e.g. DynamicLinqDataAccessor