# Architecture Decisions

## Generic Repository Rule

### When to use it?
Use `IGenericRepository<T, TKey>` only for simple, single-table operations:
* Get by ID (`GetById`)
* Get all records (`GetAll`)
* Basic CRUD: Add, Update, or Remove

---

### When NOT to use it?
Do NOT use the Generic Repository (write custom logic in a Service or Specific Repository instead) for:
* Joining multiple tables together.
* Advanced searching, filtering, or pagination.
* Complex business rules or financial transactions.