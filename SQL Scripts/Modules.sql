GO
SET IDENTITY_INSERT [dbo].[Modules] ON 

--PBJSnap application module
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (1, N'Role Management', 0, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (2, N'User Management', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (3, N'Configuration', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (4, N'Organization', 3, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (5, N'Facility', 3, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (6, N'Agency', 3, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)

INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (7, N'Dashboard', 0, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (8, N'Get Started', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (9, N'UploadSection', 8, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (10, N'Review', 8, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (11, N'SubmitSection', 8, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (12, N'SubmissionHistory', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)

INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (13, N'Storage', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (14, N'Help', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (15, N'Admin', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)

--Inventory application module
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (16, N'Role Management', 0, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (17, N'User Management', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (18, N'Shipment Management', 0, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (19, N'Stock Management', 13, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (20, N'Rental Management', 13, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (21, N'Product', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (22, N'Inventory', 0, N'ri-shield-user-line', N'/ThinkAnew/Roles/index', N'Y', CAST(N'2023-01-04T00:28:09.1400000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (23, N'Store', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (24, N'Dashboard', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,2)

INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (25, N'EmployeeMaster', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (26, N'Backout', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,1)

INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (27, N'Reports', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (28, N'Voice', 27, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (29, N'Data', 27, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (30, N'Data Service', 0, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (31, N'Upload Client File', 27, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (32, N'Carrier', 30, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
INSERT [dbo].[Modules] ([ModuleId], [Name], [Parent], [icon], [Path], [IsDisplay], [CreatedDate], [UpdatedDate], [DeletedAt], [CreatedBy], [UpdatedBy], [IsActive],[ApplicationId]) VALUES (33, N'DataServiceType', 30, N'ri-user-line', N'', N'Y', CAST(N'2023-01-04T00:28:35.0900000' AS DateTime2), NULL, NULL, NULL, NULL, 1,3)
SET IDENTITY_INSERT [dbo].[Modules] ON
GO
