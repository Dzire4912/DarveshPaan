drop procedure if exists [sp_loaddatainto_kronosTimesheetMapped_wo_date] 
go
CREATE procedure [dbo].[sp_loaddatainto_kronosTimesheetMapped_wo_date] 
(@facility_id nvarchar(50),
@OrgId int = 0)

--exec [sp_loaddatainto_kronosTimesheetMapped_wo_date] '00090'
as
--declare 
--@facility_id nvarchar(50) = '00090',
--@OrgId int = 0

begin
declare @result int;
begin transaction
begin try

declare @facilityId int = (select id from Facilities where FacilityID = @facility_id and OrganizationId = @OrgId);
declare @has_deduction bit = (select HasDeduction from Facilities where FacilityID = @facility_id and OrganizationId = @OrgId);
declare @btwn8and16 decimal(18,2) = (select RegularTime/60.0 from BreakDeductions where FacilityId = @facilityId);
declare @above16 decimal(18,2) = (select RegularTime/60.0 from BreakDeductions where FacilityId = @facilityId);
if (@btwn8and16 = 0 or @btwn8and16 is null) begin set @btwn8and16 = 0.5 end;
if (@above16 = 0 or @above16 is null) begin set @above16 = 1 end;

--Removing data for the kronosTimesheetMapped table according to the parameter
delete from kronosTimesheetMapped where FacilityId = @facilityId

--inserting data into kronosTimesheetMapped from the KronosPunchExport and the mapping data
if(@has_deduction = 'true')
begin
insert into kronosTimesheetMapped(EmployeeId,FirstName,PayTypeCode,JobTitleCode,Month,ReportQuarter,Workday,THours,FacilityId,UploadType,Year,CreateDate,Createby,OrganizationId)
SELECT    k.EmployeeID AS EmployeeId
		, k.EmployeeName AS FirstName
		, CAST(p.Paytype AS INT) AS PayTypeCode
		, jc.PBJJobCode AS JobTitleCode
		, m.Id AS Month
		, m.Quarter AS ReportQuarter
		, CONVERT(date, k.ADJUSTEDAPPLYDATE, 120) AS Workday
		, case when k.TIMEINHOURS < 8 then k.TIMEINHOURS
			   when k.TIMEINHOURS >= 8 and k.TIMEINHOURS < 16 then (k.TIMEINHOURS - @btwn8and16)
			   when k.TIMEINHOURS >= 16 then (k.TIMEINHOURS - @above16)
		end AS THours
		, j.Id AS FacilityId
		, 3 AS UploadType
		, YEAR(k.ADJUSTEDAPPLYDATE) AS Year
		, GETDATE() AS CreateDate
		, 'AutoSync' AS Createby
		, J.OrganizationId
FROM  KronosPunchExport k
INNER JOIN  KronosPaytypeMappings p ON k.PayCode = p.kronosPaytype
INNER JOIN  Facilities j ON k.facilityId = j.FacilityID
INNER JOIN  Months m ON MONTH(k.ADJUSTEDAPPLYDATE) = m.Id
INNER JOIN  KronosToPbj jc ON k.JOBCODE = jc.Code AND k.HrJob = jc.Job and right(k.DEPARTMENTNUMBER,3) = jc.DepartmentCode
WHERE CHARINDEX(k.facilityId, @facility_id) > 0 AND K.TIMEINHOURS > 0 AND J.OrganizationId = @OrgId
end
else
begin
insert into kronosTimesheetMapped(EmployeeId,FirstName,PayTypeCode,JobTitleCode,Month,ReportQuarter,Workday,THours,FacilityId,UploadType,Year,CreateDate,Createby,OrganizationId)
SELECT    k.EmployeeID AS EmployeeId
		, k.EmployeeName AS FirstName
		, CAST(p.Paytype AS INT) AS PayTypeCode
		, jc.PBJJobCode AS JobTitleCode
		, m.Id AS Month
		, m.Quarter AS ReportQuarter
		, CONVERT(date, k.ADJUSTEDAPPLYDATE, 120) AS Workday
		, k.TIMEINHOURS AS THours
		, j.Id AS FacilityId
		, 3 AS UploadType
		, YEAR(k.ADJUSTEDAPPLYDATE) AS Year
		, GETDATE() AS CreateDate
		, 'AutoSync' AS Createby
		, J.OrganizationId
FROM  KronosPunchExport k
INNER JOIN  KronosPaytypeMappings p ON k.PayCode = p.kronosPaytype
INNER JOIN  Facilities j ON k.facilityId = j.FacilityID
INNER JOIN  Months m ON MONTH(k.ADJUSTEDAPPLYDATE) = m.Id
INNER JOIN  KronosToPbj jc ON k.JOBCODE = jc.Code AND k.HrJob = jc.Job and right(k.DEPARTMENTNUMBER,3) = jc.DepartmentCode
WHERE CHARINDEX(k.facilityId, @facility_id) > 0 AND K.TIMEINHOURS > 0 AND J.OrganizationId = @OrgId
end
commit transaction;
set @result= 1; 
end try
begin catch
rollback transaction;
set @result= 0; 
end catch
select @result as Result;
end;