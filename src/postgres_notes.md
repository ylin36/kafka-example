﻿Postgresql notes

# CLI notes
Login to a host, and specify a database to login. Default database would be postgres. Get in to get more databases

As the default configuration of Postgres is, a user called postgres is made on and the user postgres has full superadmin access to entire PostgreSQL instance running on your OS.

To create a new db, and user and password
go to cli
```
sudo -u postgres psql
postgres=# create database kafkapubsub;
postgres=# create user kafkapubsubuser with encrypted password 'kafkapubsubpassword';
postgres=# grant all privileges on database kafkapubsub to kafkapubsubuser;
```

as of pg 15, you must explicitly 
\c EXAMPLE_DB postgres
# You are now connected to database "EXAMPLE_DB" as user "postgres".
GRANT ALL ON SCHEMA public TO EXAMPLE_USER;
```
\c kafkapubsub postgres
GRANT ALL ON SCHEMA public TO kafkapubsubuser;
```

to alter password
```
alter user <username> with encrypted password '<password>';
```

## Design decision: choosing between char(n), varchar(n), varchar, text
```
char(n) – takes too much space when dealing with values shorter than n (pads them to n), and can lead to subtle errors because of adding trailing spaces, plus it is problematic to change the limit
varchar(n) – it's problematic to change the limit in live environment (requires exclusive lock while altering table)
varchar – just like text
text – for me a winner – over (n) data types because it lacks their problems, and over varchar – because it has distinct name
```

# data types
https://www.postgresql.org/docs/current/datatype.html

# constraints
https://www.postgresql.org/docs/current/ddl-constraints.html

## foreign key constraint

4 ways to define it

### Inline without mentioning the target column:
```
CREATE TABLE tests 
( 
   subject_id SERIAL,
   subject_name text,
   highestStudent_id integer REFERENCES students
);
```
### Inline with mentioning the target column:
```
CREATE TABLE tests 
( 
   subject_id SERIAL,
   subject_name text,
   highestStudent_id integer REFERENCES students (student_id)
);
```
### Out of line inside the create table:
```
CREATE TABLE tests 
( 
  subject_id SERIAL,
  subject_name text,
  highestStudent_id integer, 
  constraint fk_tests_students
     foreign key (highestStudent_id) 
     REFERENCES students (student_id)
);
```
### As a separate alter table statement:

```
CREATE TABLE tests 
( 
  subject_id SERIAL,
  subject_name text,
  highestStudent_id integer
);

alter table tests 
    add constraint fk_tests_students
    foreign key (highestStudent_id) 
    REFERENCES students (student_id);
```

naming convention
FK_ForeignKeyTable_PrimaryKeyTable

## unique constraint
end of create table
```
constraint foo_uq unique (code, label));
```
new line separate from create table
```
create unique index foo_idx on foo using btree (code, label);    
```
part of the column if single column
```
create table master (
    con_id integer unique,
    ind_id integer
);
```

# GENERATED ALWAYS vs GENERATED BY DEFAULT
column_name type GENERATED { ALWAYS | BY DEFAULT } AS IDENTITY[ ( sequence_option ) ]
The GENERATED ALWAYS instructs PostgreSQL to always generate a value for the identity column. If you attempt to insert (or update) values into the GENERATED ALWAYS AS IDENTITY column, PostgreSQL will issue an error.
The GENERATED BY DEFAULT also instructs PostgreSQL to generate a value for the identity column. However, if you supply a value for insert or update, PostgreSQL will use that value to insert into the identity column instead of using the system-generated value.

# Defered keyword
DEFERRED CONSTRAINTS are useful when you know that in a transaction you'll have inconsistent data for a while, like foreign keys that don't match, but you know that at the end of a transaction it will be consistent.

