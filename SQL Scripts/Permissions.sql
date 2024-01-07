SET IDENTITY_INSERT [dbo].[Permissions] ON 

--PBJSnap application permissions
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (1, 1, N'Permissions.PBJSnap.RoleManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (2, 1, N'Permissions.PBJSnap.RoleManagement.Create', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (3, 1, N'Permissions.PBJSnap.RoleManagement.Edit', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (4, 1, N'Permissions.PBJSnap.RoleManagement.Delete', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (5, 2, N'Permissions.PBJSnap.UserManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (6, 2, N'Permissions.PBJSnap.UserManagement.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (7, 2, N'Permissions.PBJSnap.UserManagement.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (8, 2, N'Permissions.PBJSnap.UserManagement.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (9, 3, N'Permissions.PBJSnap.Configuration.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (10, 3, N'Permissions.PBJSnap.Configuration.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (11, 3, N'Permissions.PBJSnap.Configuration.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (12, 3, N'Permissions.PBJSnap.Configuration.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (13, 4, N'Permissions.PBJSnap.Organization.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (14, 4, N'Permissions.PBJSnap.Organization.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (15, 4, N'Permissions.PBJSnap.Organization.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (16, 4, N'Permissions.PBJSnap.Organization.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (17, 5, N'Permissions.PBJSnap.Facility.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (18, 5, N'Permissions.PBJSnap.Facility.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (19, 5, N'Permissions.PBJSnap.Facility.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (20, 5, N'Permissions.PBJSnap.Facility.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (21, 6, N'Permissions.PBJSnap.Agency.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (22, 6, N'Permissions.PBJSnap.Agency.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (23, 6, N'Permissions.PBJSnap.Agency.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (24, 6, N'Permissions.PBJSnap.Agency.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (25, 7, N'Permissions.PBJSnap.Dashboard.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (26, 7, N'Permissions.PBJSnap.Dashboard.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (27, 7, N'Permissions.PBJSnap.Dashboard.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (28, 7, N'Permissions.PBJSnap.Dashboard.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (29, 8, N'Permissions.PBJSnap.GetStarted.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (30, 8, N'Permissions.PBJSnap.GetStarted.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (31, 8, N'Permissions.PBJSnap.GetStarted.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (32, 8, N'Permissions.PBJSnap.GetStarted.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (33, 9, N'Permissions.PBJSnap.Upload.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (34, 9, N'Permissions.PBJSnap.Upload.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (35, 9, N'Permissions.PBJSnap.Upload.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (36, 9, N'Permissions.PBJSnap.Upload.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (37, 10, N'Permissions.PBJSnap.Review.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (38, 10, N'Permissions.PBJSnap.Review.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (39, 10, N'Permissions.PBJSnap.Review.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (40, 10, N'Permissions.PBJSnap.Review.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (41, 11, N'Permissions.PBJSnap.Submit.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (42, 11, N'Permissions.PBJSnap.Submit.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (43, 11, N'Permissions.PBJSnap.Submit.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
-- INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (44, 11, N'Permissions.PBJSnap.Submit.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (45, 12, N'Permissions.PBJSnap.SubmissionHistory.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (46, 12, N'Permissions.PBJSnap.SubmissionHistory.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (47, 12, N'Permissions.PBJSnap.SubmissionHistory.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (48, 12, N'Permissions.PBJSnap.SubmissionHistory.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (49, 12, N'Permissions.PBJSnap.SubmissionHistory.Download', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (50, 13, N'Permissions.PBJSnap.Storage.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (51, 13, N'Permissions.PBJSnap.Storage.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (52, 13, N'Permissions.PBJSnap.Storage.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (53, 13, N'Permissions.PBJSnap.Storage.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (54, 13, N'Permissions.PBJSnap.Storage.Download', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (55, 13, N'Permissions.PBJSnap.Storage.Upload', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (56, 14, N'Permissions.PBJSnap.Help.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (57, 14, N'Permissions.PBJSnap.Help.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (58, 14, N'Permissions.PBJSnap.Help.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (59, 14, N'Permissions.PBJSnap.Help.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

--Inventory application permissions
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (60, 16, N'Permissions.Inventory.RoleManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (61, 16, N'Permissions.Inventory.RoleManagement.Create', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (62, 16, N'Permissions.Inventory.RoleManagement.Edit', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (63, 16, N'Permissions.Inventory.RoleManagement.Delete', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (64, 17, N'Permissions.Inventory.UserManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (65, 17, N'Permissions.Inventory.UserManagement.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (66, 17, N'Permissions.Inventory.UserManagement.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (67, 17, N'Permissions.Inventory.UserManagement.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (68, 18, N'Permissions.Inventory.ShpimentManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (69, 18, N'Permissions.Inventory.ShpimentManagement.Create', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (70, 18, N'Permissions.Inventory.ShpimentManagement.Edit', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (71, 18, N'Permissions.Inventory.ShpimentManagement.Delete', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (72, 19, N'Permissions.Inventory.StockManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (73, 19, N'Permissions.Inventory.StockManagement.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (74, 19, N'Permissions.Inventory.StockManagement.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (75, 19, N'Permissions.Inventory.StockManagement.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (76, 20, N'Permissions.Inventory.RentalManagement.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (77, 20, N'Permissions.Inventory.RentalManagement.Create', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (78, 20, N'Permissions.Inventory.RentalManagement.Edit', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (79, 20, N'Permissions.Inventory.RentalManagement.Delete', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (80, 21, N'Permissions.Inventory.Product.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (81, 21, N'Permissions.Inventory.Product.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (82, 21, N'Permissions.Inventory.Product.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (83, 21, N'Permissions.Inventory.Product.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (84, 22, N'Permissions.Inventory.Inventory.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (85, 22, N'Permissions.Inventory.Inventory.Create', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (86, 22, N'Permissions.Inventory.Inventory.Edit', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (87, 22, N'Permissions.Inventory.Inventory.Delete', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (88, 23, N'Permissions.Inventory.Store.View', CAST(N'2023-01-06T12:47:31.3166667' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (89, 23, N'Permissions.Inventory.Store.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (90, 23, N'Permissions.Inventory.Store.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (91, 23, N'Permissions.Inventory.Store.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (92, 24, N'Permissions.Inventory.Dashboard.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (93, 24, N'Permissions.Inventory.Dashboard.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (94, 24, N'Permissions.Inventory.Dashboard.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (95, 24, N'Permissions.Inventory.Dashboard.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (96, 25, N'Permissions.PBJSnap.EmployeeMaster.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (97, 25, N'Permissions.PBJSnap.EmployeeMaster.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (98, 25, N'Permissions.PBJSnap.EmployeeMaster.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (99, 25, N'Permissions.PBJSnap.EmployeeMaster.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (100, 26, N'Permissions.PBJSnap.Backout.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (101, 26, N'Permissions.PBJSnap.Backout.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (102, 26, N'Permissions.PBJSnap.Backout.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (103, 26, N'Permissions.PBJSnap.Backout.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (104, 11, N'Permissions.PBJSnap.Submit.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (105, 11, N'Permissions.PBJSnap.Submit.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (106, 9, N'Permissions.PBJSnap.Upload.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (107, 9, N'Permissions.PBJSnap.Upload.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (108, 10, N'Permissions.PBJSnap.Review.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (109, 10, N'Permissions.PBJSnap.Review.AddCensus', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (110, 10, N'Permissions.PBJSnap.Review.EditCensus', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (111, 10, N'Permissions.PBJSnap.Review.DeleteCensus', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (112, 10, N'Permissions.PBJSnap.Review.Approve', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (113, 26, N'Permissions.PBJSnap.Backout.DeleteInvalidaRecord', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (114, 26, N'Permissions.PBJSnap.Backout.ViewInvalidRecord', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (115, 10, N'Permissions.PBJSnap.Review.Validate', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (116, 27, N'Permissions.TelecomReporting.Reports.Add', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (117, 27, N'Permissions.TelecomReporting.Reports.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (118, 27, N'Permissions.TelecomReporting.Reports.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (119, 27, N'Permissions.TelecomReporting.Reports.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (120, 28, N'Permissions.TelecomReporting.Voice.Add', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (121, 28, N'Permissions.TelecomReporting.Voice.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (122, 28, N'Permissions.TelecomReporting.Voice.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (123, 28, N'Permissions.TelecomReporting.Voice.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (124, 29, N'Permissions.TelecomReporting.Data.Add', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (125, 29, N'Permissions.TelecomReporting.Data.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (126, 29, N'Permissions.TelecomReporting.Data.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
--INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (127, 29, N'Permissions.TelecomReporting.Data.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (128, 30, N'Permissions.TelecomReporting.DataService.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (129, 30, N'Permissions.TelecomReporting.DataService.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (130, 30, N'Permissions.TelecomReporting.DataService.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (131, 30, N'Permissions.TelecomReporting.DataService.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (132, 31, N'Permissions.Reporting.UploadClientFile.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (133, 31, N'Permissions.Reporting.UploadClientFile.Download', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (134, 31, N'Permissions.Reporting.UploadClientFile.Upload', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (135, 31, N'Permissions.Reporting.UploadClientFile.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (136, 31, N'Permissions.Reporting.UploadClientFile.FileView', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (137, 32, N'Permissions.Reporting.Carrier.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (138, 32, N'Permissions.Reporting.Carrier.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (139, 32, N'Permissions.Reporting.Carrier.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (140, 32, N'Permissions.Reporting.Carrier.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (141, 27, N'Permissions.Reporting.Reports.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (142, 27, N'Permissions.Reporting.Reports.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (143, 27, N'Permissions.Reporting.Reports.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (144, 27, N'Permissions.Reporting.Reports.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (145, 28, N'Permissions.Reporting.Voice.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (146, 28, N'Permissions.Reporting.Voice.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (147, 28, N'Permissions.Reporting.Voice.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (148, 28, N'Permissions.Reporting.Voice.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (149, 28, N'Permissions.Reporting.Voice.Export', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (150, 29, N'Permissions.Reporting.Data.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (151, 29, N'Permissions.Reporting.Data.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (152, 29, N'Permissions.Reporting.Data.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (153, 29, N'Permissions.Reporting.Data.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (154, 29, N'Permissions.Reporting.DataServiceType.Create', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (155, 29, N'Permissions.Reporting.DataServiceType.View', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (156, 29, N'Permissions.Reporting.DataServiceType.Delete', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)
INSERT [dbo].[Permissions] ([Id], [ModuleId], [Name], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive]) VALUES (157, 29, N'Permissions.Reporting.DataServiceType.Edit', CAST(N'2023-01-06T13:18:52.1500000' AS DateTime2), NULL, NULL, NULL, 1)

SET IDENTITY_INSERT [dbo].[Permissions] ON
GO