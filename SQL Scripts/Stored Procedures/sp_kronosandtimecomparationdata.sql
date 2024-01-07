
drop procedure if exists [dbo].[sp_kronosandtimecomparationdata]
go
Create procedure [dbo].[sp_kronosandtimecomparationdata]
(
@start_date date,
@end_date date,
@OrgId int = 0,
@FacilityId int=0,
@current_quarter int = null,
@last_quarter int = null
)
as 

--declare 
--@start_date date = '2023-09-15 00:00:00.000',
--@end_date date = '2023-12-13 00:00:00.0000',
--@current_quarter int = null,
--@last_quarter int = null,
--@OrgId int = 1;

--execute sp_kronosandtimecomparationdata '2023-08-25 00:00:00.000','2023-11-22 00:00:00.000'

begin
begin transaction;
begin try

--timesheet or update: result data move to validate update
drop table if exists #rowidcapturetable;
select distinct KR.rowid, TT.id
into #rowidcapturetable
from kronosTimesheetMapped KR --rowid added kronos
inner join Timesheet TT --actual time sheet table
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
where KR.Workday between @start_date and @end_date
and KR.OrganizationId = @OrgId and TT.FacilityId=@FacilityId
group by KR.rowid, TT.id;

--need to insert into timesheet instance result display as table output
--insert into Timesheet(FirstName,EmployeeId,FacilityId,PayTypeCode,JobTitleCode,Workday,THours)
select FirstName as FullName,EmployeeId,FacilityId,PayTypeCode,JobTitleCode,format(cast(Workday as datetime),'MM-dd-yyyy')  as Workday,THours
from kronosTimesheetMapped 
where rowid not in (select rowid from #rowidcapturetable)
and OrganizationId = @OrgId and FacilityId=@FacilityId;

if exists (select 1 from #rowidcapturetable)
begin
--Multiple record with same thour fail case scenario handled
drop table if exists #duplicateoccursinkronos;
select EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode, sum(THours) as sumofthours, count(THours) as row_count
into #duplicateoccursinkronos
from kronosTimesheetMapped 
where rowId in (select distinct rowid from #rowidcapturetable) and FacilityId=@FacilityId
group by EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode;

drop table if exists #duplicateoccursintimesheet;
select EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode, sum(THours) as sumofthours, count(THours) as row_count
into #duplicateoccursintimesheet
from Timesheet 
where Id in (select distinct id from #rowidcapturetable) and FacilityId=@FacilityId
group by EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode;

drop table if exists #updatedrecords
create table #updatedrecords(EmployeeId nvarchar(50),FacilityId int,Workday datetime2)
insert into #updatedrecords(EmployeeId,FacilityId,Workday)
select EmployeeId,FacilityId,Workday
From (
--Total Hours Variation
select KR.EmployeeId,KR.FacilityId,KR.Workday
from #duplicateoccursinkronos KR
inner join #duplicateoccursintimesheet TT 
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
and KR.PayTypeCode = TT.PayTypeCode
and KR.JobTitleCode = TT.JobTitleCode
and (KR.sumofthours <> TT.sumofthours or KR.row_count <> TT.row_count)
group by KR.EmployeeId,KR.FacilityId,KR.Workday
union all
--PayTypeCode Variation
select KR.EmployeeId,KR.FacilityId,KR.Workday
from #duplicateoccursinkronos KR
inner join #duplicateoccursintimesheet TT 
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
and KR.PayTypeCode <> TT.PayTypeCode
group by KR.EmployeeId,KR.FacilityId,KR.Workday
union all
--JobTitleCode Variation
select KR.EmployeeId,KR.FacilityId,KR.Workday
from #duplicateoccursinkronos KR
inner join #duplicateoccursintimesheet TT 
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
and KR.PayTypeCode = TT.PayTypeCode
and KR.sumofthours = TT.sumofthours 
and KR.JobTitleCode <> TT.JobTitleCode  
group by KR.EmployeeId,KR.FacilityId,KR.Workday
) as R;

drop table if exists #rowidcapturetableupdated
select distinct KR.rowid
into #rowidcapturetableupdated
from kronosTimesheetMapped KR --rowid added kronos
inner join #updatedrecords TT --actual time sheet table
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
where KR.OrganizationId = @OrgId and kr.FacilityId=@FacilityId

--General non matching records
drop table if exists #updatedTempRecord
select KR.rowid 
into #updatedTempRecord
from kronosTimesheetMapped KR
--comparing with actual time sheet table
inner join Timesheet TT 
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
and KR.THours = TT.THours
and KR.PayTypeCode = TT.PayTypeCode
and KR.JobTitleCode = TT.JobTitleCode
where KR.rowid in (select distinct rowid from #rowidcapturetable) and KR.OrganizationId = @OrgId and kr.FacilityId=@FacilityId

--Deleting the existing records from pending record tables as per the min and max date
delete from kronosPendingRecords where Workday between @start_date and @end_date and OrgId = @OrgId;

--Inserting the need to update record details inside the kronosPendingRecords table
insert into kronosPendingRecords (EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrgId)
select EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrganizationId
from (
select k.EmployeeId,k.FacilityId,k.Workday,k.FirstName as EmployeeName,f.FacilityName, f.OrganizationId
from kronosTimesheetMapped k
join Facilities f on k.FacilityId=f.Id
where k.rowid in (select rowid from #rowidcapturetableupdated) and k.OrganizationId = @OrgId and k.FacilityId=@FacilityId
group by k.EmployeeId,k.FacilityId,k.Workday,k.FirstName,f.FacilityName,f.OrganizationId
union all
select k.EmployeeId,k.FacilityId,k.Workday,k.FirstName as EmployeeName,f.FacilityName, f.OrganizationId
from kronosTimesheetMapped k
inner join Timesheet T
on k.EmployeeId = T.EmployeeId
and k.FacilityId = T.FacilityId
and k.Workday = T.Workday
join Facilities f on k.FacilityId=f.Id
where k.rowid not in (select distinct rowid from #updatedTempRecord)
and k.OrganizationId = @OrgId and k.FacilityId=@FacilityId
group by k.EmployeeId,k.FacilityId,k.Workday,k.FirstName,f.FacilityName,f.OrganizationId) as Result
group by EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrganizationId
end
commit transaction;
end try
begin catch
rollback transaction;
end catch
end;


