
drop procedure if exists [dbo].[sp_kronosandtimecomparationdata_wo_date_w_facility];
go

create procedure [dbo].[sp_kronosandtimecomparationdata_wo_date_w_facility]
(@facility_id nvarchar(50),@OrgId int = 0)
as 

--declare 
--@facility_id nvarchar(50) = '00090',
--@OrgId int = 0;

--execute sp_kronosandtimecomparationdata_wo_date_w_facility '00090' ,1

begin
begin transaction;
begin try

declare @facilityId int = (select id from Facilities where FacilityID = @facility_id and OrganizationId = @OrgId);

--timesheet or update: result data move to validate update
drop table if exists #rowidcapturetable;
select distinct KR.rowid, TT.id
into #rowidcapturetable
from kronosTimesheetMapped KR --rowid added kronos
inner join Timesheet TT --actual time sheet table
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
where KR.FacilityId = @facilityid
group by KR.rowid, TT.id;

--need to insert into timesheet instance result display as table output
--insert into Timesheet(FirstName,EmployeeId,FacilityId,PayTypeCode,JobTitleCode,Workday,THours)
select FirstName as FullName,EmployeeId,FacilityId,PayTypeCode,JobTitleCode,format(cast(Workday as datetime),'MM-dd-yyyy')  as Workday,THours
from kronosTimesheetMapped 
where FacilityId = @facilityid
and rowid not in (select rowid from #rowidcapturetable);

if exists (select 1 from #rowidcapturetable)
begin
--Multiple record with same thour fail case scenario handled
drop table if exists #duplicateoccursinkronos;
select EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode, sum(THours) as sumofthours, count(THours) as row_count
into #duplicateoccursinkronos
from kronosTimesheetMapped 
where FacilityId = @facilityid
and rowId in (select distinct rowid from #rowidcapturetable)
group by EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode;

drop table if exists #duplicateoccursintimesheet;
select EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode, sum(THours) as sumofthours, count(THours) as row_count
into #duplicateoccursintimesheet
from Timesheet 
where FacilityId = @facilityid
and Id in (select distinct id from #rowidcapturetable)
group by EmployeeId,FacilityId,Workday,PayTypeCode,JobTitleCode;

drop table if exists #updatedrecords;
create table #updatedrecords(EmployeeId nvarchar(50),FacilityId int,Workday datetime2)
insert into #updatedrecords(EmployeeId,FacilityId,Workday)
select EmployeeId,FacilityId,Workday
From (
-- same record only variation in Total Hours
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

drop table if exists #rowidcapturetableupdated;
select distinct KR.rowid
into #rowidcapturetableupdated
from kronosTimesheetMapped KR --rowid added kronos
inner join #updatedrecords TT --actual time sheet table
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
where KR.FacilityId = @facilityid;

--General non matching records
drop table if exists #updatedTempRecord;
select KR.rowid into #updatedTempRecord
from kronosTimesheetMapped KR
--comparing with actual time sheet table
inner join Timesheet TT 
on KR.EmployeeId = TT.EmployeeId
and KR.FacilityId = TT.FacilityId
and KR.Workday = TT.Workday
and KR.THours = TT.THours
and KR.PayTypeCode = TT.PayTypeCode
and KR.JobTitleCode = TT.JobTitleCode
where KR.FacilityId = @facilityid
and KR.rowid in (select distinct rowid from #rowidcapturetable);

--Deleting the existing records from pending record tables as per the min and max date
delete from kronosPendingRecords where FacilityId = @facilityId;

--Inserting the need to update record details inside the kronosPendingRecords table
insert into kronosPendingRecords (EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrgId)
select EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrganizationId
from (
select k.EmployeeId,k.FacilityId,k.Workday,k.FirstName as EmployeeName,f.FacilityName,f.OrganizationId
from kronosTimesheetMapped k
join Facilities f on k.FacilityId=f.Id
where k.FacilityId = @facilityid
and k.rowid in (select rowid from #rowidcapturetableupdated)
group by k.EmployeeId,k.FacilityId,k.Workday,k.FirstName,f.FacilityName,f.OrganizationId
union all
select k.EmployeeId,k.FacilityId,k.Workday,k.FirstName as EmployeeName,f.FacilityName,f.OrganizationId
from kronosTimesheetMapped k
inner join Timesheet T
on k.EmployeeId = T.EmployeeId
and k.FacilityId = T.FacilityId
and k.Workday = T.Workday
join Facilities f on k.FacilityId=f.Id
where K.FacilityId = @facilityid
and k.rowid not in (select distinct rowid from #updatedTempRecord)
group by k.EmployeeId,k.FacilityId,k.Workday,k.FirstName,f.FacilityName,f.OrganizationId) as Result
group by EmployeeId,FacilityId,Workday,EmployeeName,FacilityName,OrganizationId;
end
commit transaction;
end try
begin catch
rollback transaction;
end catch
end;