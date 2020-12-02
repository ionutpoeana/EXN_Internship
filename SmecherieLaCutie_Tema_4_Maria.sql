-- Smecherie la Cutie : Alexandra's homework 

--1 In table HistoryComponents add a single column 'DaysInUse' representing the usable period for
-- the row (represented in days)

alter table ComponentHistories add DaysInUse int;
go
	update ComponentHistories
		set DaysInUse = DATEDIFF(DAY,ISNULL(EndDate,SYSDATETIME()),StartDate);

SELECT ID, DaysInUse FROM ComponentHistories;

--1.1 Display the number of components which AS marked AS permanently out of use.

SELECT Count(dc.ID) 'Components permanently out of use'
	FROM DefectiveComponents dc
	WHERE dc.ReparationStatus = 3;
	

--2  Calculate the minim, maxim and the average time of use of a component. It will be displayed: Id, Name,
--  Max, Min, Average.
SELECT c.ID, ct.Name, MIN(ch.DaysInUse) AS Min, MAX(ch.DaysInUse) AS Max, AVG(ch.DaysInUse) AS Average 
	FROM Components c 
		inner join ComponentHistories ch ON c.ID = ch.ComponentID
		inner join ComponentTypes ct ON ct.ID = c.TypeID
	GROUP BY C.ID, ct.Name;

--3 Display the average cost of component requests in the last 12 months.
SELECT AVG(EstimatedPrice)  AS AverageCost
	FROM ComponentRequests
	WHERE RequestDate > DATEADD(YEAR,-1, SYSDATETIME());
	
--4 Display the number of employees with role 3 (role id = 3) which are curently using a subcomponent.
SELECT COUNT(*) AS EmployeesNumber
	FROM Employees e 
		inner join ComponentHistories ch ON e.ID = ch.EmployeeID 
		inner join Components c ON ch.ComponentID = c.ID 
	WHERE e.RoleID = 3 and  c.ParentID is not null and ch.EndDate > GETDATE();

--5 Display the distinct Name and Brand of the components with the last use time lower than current date
-- - 3 months
SELECT distinct cm.name, cb.Name as BrandName
	FROM ComponentHistories ch
		inner join Components c ON ch.ComponentID = c.ID
		inner join ComponentModels cm ON c.ModelID = cm.ID
		inner join ComponentBrands cb ON cm.BrandID = cb.ID
	WHERE ch.EndDate < DATEADD(MONTH,-3,GETDATE())
	GROUP BY cm.name, cb.name;
	

--6 Display the number of defective independent components
SELECT COUNT(*) AS 'Defective components'
	FROM DefectiveComponents dc 
		inner join Components c ON dc.ComponentID = c.ID 
	WHERE c.ParentID is null and not (exists(
		SELECT 1
		FROM Components c1 
		WHERE c.ID = c1.ParentID )); 

--7 Display the brand name and the number of components in each brand ORDER BY  number of components descending
SELECT cb.Name as BrandName, COUNT(c.ID) AS Count
	FROM ComponentBrands cb
		inner join ComponentModels cm ON cb.ID = cm.BrandID
		inner join Components c ON cm.ID = c.ModelID
	GROUP BY cb.Name
	ORDER BY  Count desc;

--8 Display the full name of employees with the name starting with 'D' and at least a surname 
-- is ending in 'a'
SELECT LastName, FirstName
	FROM Employees
	WHERE LastName like 'D%' and ( LastName like '%a' or FirstName like '%a');

--9 Display the total amount of spent money in the last YEARONcomponents(approved reqeusts)
SELECT SUM(EstimatedPrice) 
	FROM ComponentRequests
	WHERE Status = 1 and RequestDate > DATEADD(YEAR,-1,SYSDATETIME());

--10 Display the Name, Mail and Phone number for all employees with a complete request between
-- May and August last YEAR.
SELECT e.FirstName, e.LastName, e.
	FROM Employees e 
		inner join ComponentRequests cr ON e.ID = cr.EmployeeID
	WHERE  cr.Status = 1 and  Month(cr.RequestDate) between 5 and 8;

--11 For each role, display the name of the role and between brackets the mail for all employees with 
-- the current role separated by ','.
SELECT r.Name as RoleName, '[' + STRING_AGG(e.email, ' , ') + ']' AS Emails
	FROM Roles r
		inner join Employees e ON r.ID = e.RoleID
	GROUP BY r.Name;

--12 Add the column 'DateOfBirth' of type date in the table Employees and display top 30 yungest employees.
alter table dbo.Employees add DateOfBirth date;
go
update Employees set DateOfBirth = DATEADD(DAY,ABS(CHECKSUM(NEWID())%10650),'1970-01-01');

SELECT top 30 LastName, FirstName, YEAR(GETDATE()) - YEAR(DateOfBirth) AS Age
	FROM Employees 
	ORDER BY  Age asc;

--13 Display the age and mail (alphabetically ordered by name) for each employee which is using a
-- component for at least 3 months 
SELECT distinct LastName, (YEAR(GETDATE())- YEAR(e.DateOfBirth)) AS Age, e.Email 
	FROM Employees e 
		inner join ComponentHistories ch ON e.ID = ch.EmployeeID
	WHERE DATEDIFF(MONTH,ch.StartDate,ch.EndDate) >= 3
	ORDER BY  LastName;

--14 Display the full name in column 'Employee' and the age in column 'Age' for the top 10 employees
-- with the biggest number of requests.
SELECT top 10 CONCAT_WS(' ', e.LastName, e.FirstName) AS Employee,
		(YEAR(SYSDATETIME())- YEAR(DateOfBirth)) AS Age, 
		count(cr.ID) AS Requests
	FROM Employees e
		inner join ComponentRequests cr ON e.ID = cr.EmployeeID 
	GROUP BY e.FirstName, e.LastName, e.DateOfBirth
	ORDER BY  count(cr.EmployeeID) desc, e.LastName;

--15 Display all employees with a coleague (an employee with the same role) which made a request for
--	a component FROM the same brand
SELECT DISTINCT	 CONCAT_WS(' ', e1.FirstName, e1.LastName) AS Employee
	FROM Employees e1
		inner join Employees e2 ON e1.ID != e2.ID and e1.RoleID = e2.RoleID

		inner join ComponentHistories ch1 ON ch1.EmployeeID = e1.ID
		inner join Components c1 ON c1.ID = ch1.ComponentID
		inner join ComponentModels cm1 ON cm1.ID = c1.ModelID
		
		inner join ComponentHistories ch2 ON ch2.EmployeeID = e2.ID
		inner join Components c2 ON c2.ID = ch2.ComponentID
		inner join ComponentModels cm2 ON cm2.ID = c2.ModelID 

	 WHERE cm1.BrandID = cm2.BrandID
	 GROUP BY e1.RoleID, e1.FirstName, e1.LastName
	 ORDER BY Employee;


--16 Display Name, Surname and Age for all employees with age between 35 and 45. Order the list by
-- the length of the name and surname.
SELECT FirstName, LastName, (YEAR(SYSDATETIME())- YEAR(DateOfBirth)) AS Age 
	FROM Employees
	WHERE DATEDIFF(YEAR,DateOfBirth,GETDATE()) between 35 and 45
	ORDER BY  LEN(FirstName), LEN(LastName);

--17 Display the Name and Surname separated by '_' for all employees with at least a 
--		colleague (same role) with the date of birth in the same month and YEAR
SELECT distinct CONCAT_WS('_',e1.FirstName,e1.LastName) AS Employee
	FROM Employees e1 
		inner join Employees e2 ON e1.RoleID = e2.RoleID and e1.ID != e2.ID
	WHERE YEAR(e1.DateOfBirth) = YEAR(e2.DateOfBirth) and
		MONTH(e1.DateOfBirth) = MONTH(e2.DateOfBirth)
	ORDER BY  Employee;

--18 Display all the employees (Name, Surname and Mail in a column 'EmployeeDetails'), the total cost of 
--   component requests (both approved and not) in the last 5 YEARs. If no requests were made, display 0.
SELECT CONCAT_WS(' ',e.FirstName, e.LastName, e.Email) AS EmployeeDetails, 
		ISNULL(sum(cr.EstimatedPrice),0) AS ComponentExpenses
	FROM Employees e 
		left outer join ComponentRequests cr ON e.ID = cr.EmployeeID
	WHERE cr.Status !=3 and cr.RequestDate > DATEADD(YEAR,-5,RequestDate)
	GROUP BY e.ID, e.LastName, e.FirstName, e.Email
	ORDER BY  ComponentExpenses;

--19 Display the name and surname of employees with surname starting with a consonant and finishing with
--   a vowel ORDER BY  date of birth 
SELECT FirstName, LastName 
	FROM Employees 
	WHERE LastName LIKE '[AEIOU]%[BCDFGHJKLMNPQRSTVWXYZ]'
	ORDER BY  DateOfBirth;

-- 20 
/*
20. Optional 
Afisati numele complet in coloana "Angajat" si parola in coloana" Password"( care se va genera dupa regulile pe
care le veti gasi mai jos)  pentru toti angajatii care au un coleg(acelasi rol) care are o componenta de acelasi
brand ca el. 

Generare parola:Daca id % 3 =
 
0 Atunci parola va fi numarul de litere al numelui concatenat cu numele complet scris invers si varsta sa

1 atunci parola va fi  numelui pozitiei(rolul) in pasareasca concatenat cu,numele cu majuscula scris invers si 
in care toate "c" sunt inlocuite cu "df" si numarul de luni de cand a facut ultimul request in coloana

2 atunci parola va fi numele ultimului model de componenta pe care l-a primit dar fara primele 2 litere si
ultima si scris apoi invers, concatenat cu numele parintelui cu majuscula sau cu textul"IamOrfan" daca nu are parinte

*/