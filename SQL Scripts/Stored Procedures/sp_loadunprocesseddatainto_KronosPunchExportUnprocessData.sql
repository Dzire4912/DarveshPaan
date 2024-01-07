drop procedure if exists [dbo].[sp_loadunprocesseddatainto_KronosPunchExportUnprocessData]
go
GO
Create procedure [dbo].[sp_loadunprocesseddatainto_KronosPunchExportUnprocessData] 
--exec sp_loadunprocesseddatainto_KronosPunchExportUnprocessData
as
--select * from KronosPunchExportUnprocessedData From the mentioned columns data not available in the master table - PayCode or JOBCODE or HrJob or DEPARTMENTNUMBER
begin
begin transaction;
begin try

delete from KronosPunchExportUnprocessedData where 1=1;
declare @result int;
drop table if exists #ProcessedData;
SELECT k.id
INTO #ProcessedData
FROM  KronosPunchExport k
INNER JOIN  Facilities j ON k.facilityId = j.FacilityID
INNER JOIN  KronosPaytypeMappings p ON k.PayCode = p.kronosPaytype
INNER JOIN  KronosToPbj jc ON k.JOBCODE = jc.Code AND k.HrJob = jc.Job and right(k.DEPARTMENTNUMBER,3) = jc.DepartmentCode
WHERE 1=1 and K.TIMEINHOURS between 0.01 and 24;
 
--Load unprocessed Data into KronosPunchExportUnprocessData
INSERT INTO KronosPunchExportUnprocessedData
(facilityId ,EmployeeID ,EmployeeName ,NATIVEKRONOSID ,DEPARTMENTNUMBER ,DepartmentDescription ,JOBCODE ,JobDescription ,ADJUSTEDAPPLYDATE ,PayCode ,TIMEINHOURS ,HrJob ,Reason,OrganizationId)
SELECT facilityId, EmployeeID, EmployeeName, NATIVEKRONOSID, DEPARTMENTNUMBER, DepartmentDescription, JOBCODE, JobDescription, ADJUSTEDAPPLYDATE, PayCode, TIMEINHOURS, HrJob, Reason,OrganizationId
FROM (
SELECT facilityId
	,EmployeeID
	,EmployeeName
	,NATIVEKRONOSID
	,DEPARTMENTNUMBER
	,DepartmentDescription
	,JOBCODE
	,JobDescription
	,ADJUSTEDAPPLYDATE
	,PayCode
	,TIMEINHOURS
	,HrJob
	,CONCAT('The following columns have either NULL values or empty strings - ',
	(case when len(isnull(facilityId,'')) = 0 then 'facilityId, ' else '' end),
	(case when len(isnull(EmployeeID,'')) = 0 then 'EmployeeID, ' else '' end),
    (case when len(isnull(PayCode,'')) = 0 then 'PayCode, ' else '' end), 
	(case when len(isnull(JOBCODE,'')) = 0 then 'JOBCODE, ' else '' end), 
	(case when len(isnull(HrJob,'')) = 0 then 'HrJob, ' else '' end), 
	(case when len(isnull(DEPARTMENTNUMBER,'')) = 0 then 'and DEPARTMENTNUMBER' else '' end)
	) as Reason,OrganizationId
FROM
    KronosPunchExport KPE
WHERE 1=1
and id not in (select id from #ProcessedData) 
and (
    len(isnull(facilityId,'')) = 0 OR
	len(isnull(EmployeeID,'')) = 0 OR
    len(isnull(PayCode,'')) = 0 OR 
	len(isnull(JOBCODE,'')) = 0 OR 
	len(isnull(HrJob,'')) = 0 OR 
	len(isnull(DEPARTMENTNUMBER,'')) = 0 )
union all
SELECT facilityId
	,EmployeeID
	,EmployeeName
	,NATIVEKRONOSID
	,DEPARTMENTNUMBER
	,DepartmentDescription
	,JOBCODE
	,JobDescription
	,ADJUSTEDAPPLYDATE
	,PayCode
	,TIMEINHOURS
	,HrJob
    ,('The TIMEINHOURS column has null values or values less than 0 or greater than 24') AS Reason,
	OrganizationId
FROM KronosPunchExport 
WHERE 1=1
and id not in (select id from #ProcessedData)
and (TIMEINHOURS is null OR TIMEINHOURS <= 0 OR TIMEINHOURS > 24 )
UNION ALL
	SELECT facilityId
	,EmployeeID
	,EmployeeName
	,NATIVEKRONOSID
	,DEPARTMENTNUMBER
	,DepartmentDescription
	,JOBCODE
	,JobDescription
	,ADJUSTEDAPPLYDATE
	,PayCode
	,TIMEINHOURS
	,HrJob
    , 'From the mentioned columns data not available in the master table - PayCode or JOBCODE or HrJob or DEPARTMENTNUMBER' AS Reason,OrganizationId
FROM
    KronosPunchExport KPE
WHERE 1=1
	and id not in (select id from #ProcessedData) 
    and (NOT EXISTS (SELECT 1 FROM KronosPaytypeMappings P WHERE P.kronosPaytype = KPE.PayCode) OR
   	NOT EXISTS (SELECT 1 FROM KronosToPbj K WHERE K.Code = KPE.JOBCODE) OR
   	NOT EXISTS (SELECT 1 FROM KronosToPbj K WHERE K.Job = KPE.HrJob) OR
   	NOT EXISTS (SELECT 1 FROM KronosToPbj K WHERE K.DepartmentCode = right(KPE.DEPARTMENTNUMBER,3)))
	) AS Validate
commit transaction;
set @result=1;

end try
begin catch
rollback transaction;
set @result=0
end catch
select @result as Result;
end;
