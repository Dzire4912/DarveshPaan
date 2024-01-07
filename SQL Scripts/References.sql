GO
INSERT INTO [dbo].[References]
           ([RefereceId]
           ,[GroupiTitle]
           ,[Groupname]
           ,[Name]
           ,[Description]
           ,[DisplayOrder]
           ,[Isdeleted])
     VALUES
           (10, 'Login', 'LoginAudit', 'LoginAudit', 'Successfully logged in', NULL, 0),
		   (100	, 'Login', 'LoginAudit', 'LockedOut', 'Has been LockedOut', NULL, 0),
		   (1000, 'Login', 'LoginAudit', 'Requires Verification','Need verification OTP or Email or Password', NULL, 0),
		   (10078, 'Login', 'LoginAudit', 'Failure', 'Login failed', NULL, 0),
		   (0, 'Submissions', 'SubmissionStatus', 'Submitted', 'Just got submited',	NULL, 0),
           (1, 'Submissions', 'SubmissionStatus', 'Pending', 'Still pending',NULL, 0),
           (2, 'Submissions', 'SubmissionStatus', 'Accepted', 'File Got Accepted', NULL, 0),
           (3, 'Submissions', 'SubmissionStatus', 'Rejected', 'File Got Rejected', NULL, 0);
GO