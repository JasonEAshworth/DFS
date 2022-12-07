# Valid_DynamicFilterSort

## Sorting

* Fields may be sorted in ascending (ASC) or decending (DESC) order. 
    ```lastname=ASC```
* Fields may be chained together for sorting using commas between fields.
    ```lastname=ASC,firstname=ASC,dateofbirth=DESC```
* Field names and sort order are not case sensitive.

## Filtering
Fields may be filtered using any of the following ways

|Operator  |Common Name             |Notes                                                                                                                       |
|:--------:|------------------------|----------------------------------------------------------------------------------------------------------------------------|
|=         |Equals                  |Exact match                                                                                                                 |
|!=        |Not Equal               |Any result that does not exactly match                                                                                      |
|>         |Greater Than            |Any result greater in value, but not including the exact match                                                              |
|<         |Less Than               |Any result lesser in value, but not including the exact match                                                               |
|>=        |Greater Than or Equal To|Any result greater in value, including the exact match                                                                      |
|<=        |Less Than or Equal To   |Any result lesser in value, including the exact match                                                                       |
|={value}% |Starts With             |Uses Equal Sign, partial match value, followed by percent sign, should be indexed and relatively fast                       |
|=%{value} |Ends With               |Uses Equal Sign, percent sign, followed by partial match value, not indexed, and should be slower                           |
|=%{value}%|Contains                |Uses Equal Sign, percent sign, followed by partial match value, and another percent sign. not indexed, and should be slower |

* Filters may be mixed and matched and chained together.
* Field names and values are not case sensitive.
* Enums may not use partial comparisions using the `%`

### Filtering Examples

**Equals**


* Single (string)
```lastname=Weasley```
* Chained
```lastname=Weasley,firstname=George```
* Number
```id=77```
* Boolean
```enrolled=true```

**Not Equals**


* Single (string)
```firstname!=Fred```


* Chained (with another operator)
```firstname!=Fred,lastname=Weasley```


* Chained Integer
```id!=77,id!=78,id!=79```

**Greater Than (or equal to), Less Than (or equal to), and ranges**


* Greater than (Date)
```birthday>1900-01-01```


* Less Than (Number)
```id<100```


* Greater than or equal to (Number)
```id>=100```


* Less than or equal to (Date)
```birthday<=2001-12-31```


* Range Inclusive (Date)
```birthday>=1990-01-01,birthday<=2001-12-31```


* Range Exculsive (Number)
```total>43.99,total<170```

**Starts With**


* String Starts With
```city=hog%```


* DateTime Starts With
```modified=%2018-11-11```

**Ends With**

* String Ends With
```city=%warts```


**Contains**


* Text contains string
```description=%wizardry%```


* DateTime contains partial date
```modified=%2018-11-11%```
