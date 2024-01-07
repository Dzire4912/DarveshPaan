INSERT [dbo].[CMSMappingFieldData] ([Id],[FieldName],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[IsActive] )
VALUES (1, 'Employee Id', getdate(),getdate(),'Manual','',1),
(2, 'Hire Date', getdate(),getdate(),'Manual','',1),
(3, 'Pay Type Code', getdate(),getdate(),'Manual','',1),
(4, 'Job Title Code', getdate(),getdate(),'Manual','',1),
(5, 'Employee Status', getdate(),getdate(),'Manual','',1),
(6, 'First Name', getdate(),getdate(),'Manual','',1),
(7, 'Last Name', getdate(),getdate(),'Manual','',1),
(8, 'Work Day', getdate(),getdate(),'Manual','',1),
(9, 'Hours', getdate(),getdate(),'Manual','',1),
(10, 'Clock Punch', getdate(),getdate(),'Manual','',1),
(11, 'Full Name', getdate(),getdate(),'Manual','',1),
(12, 'Full Name with First Name first', getdate(),getdate(),'Manual','',1),
(13, 'Full Name with Last Name first', getdate(),getdate(),'Manual','',1),
(14, 'Termination Date', getdate(),getdate(),'Manual','',1)

----------------------------------------------------------------------------------------------------------------------------------------------
INSERT INTO [dbo].[PayTypeCodes]
           ( [PayTypeCode],[PayTypeDescription],[CreateDate],[CreateBy],[UpdateDate],[UpdateBy],[IsActive])
     VALUES
           ( 1,'Exempt',getdate(),'Manual',getdate(),'Manual',1),
           ( 2,'Non-Exempt',getdate(),'Manual',getdate(),'Manual',1),
           ( 3,'Contract',getdate(),'Manual',getdate(),'Manual',1)

----------------------------------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[LaborCodes]([Id],[Description],[CreateDate],[CreateBy],[UpdateDate],[UpdateBy],[IsActive])
     VALUES (1,'Administration Services',getdate(),'Manual',getdate(),'Manual',1),
      (2,'Physician Services',getdate(),'Manual',getdate(),'Manual',1),
      (3,'Nursing Services',getdate(),'Manual',getdate(),'Manual',1),
      (4,'Pharmacy Services',getdate(),'Manual',getdate(),'Manual',1),
      (5,'Dietary Services',getdate(),'Manual',getdate(),'Manual',1),
      (6,'8 Therapeutic Services',getdate(),'Manual',getdate(),'Manual',1),
      (7,'Dental Services',getdate(),'Manual',getdate(),'Manual',1),
      (8,'Podiatry Services',getdate(),'Manual',getdate(),'Manual',1),
      (9,'Mental Health Services',getdate(),'Manual',getdate(),'Manual',1),
      (10,'Vocational Services',getdate(),'Manual',getdate(),'Manual',1),
      (11,'Clinical Laboratory Services',getdate(),'Manual',getdate(),'Manual',1),
      (12,'Diagnostic X-ray Services',getdate(),'Manual',getdate(),'Manual',1),
      (13,'Blood Service Worker',getdate(),'Manual',getdate(),'Manual',1),
      (14,'Housekeeping Services',getdate(),'Manual',getdate(),'Manual',1),
      (15,'Other Services',getdate(),'Manual',getdate(),'Manual',1)

----------------------------------------------------------------------------------------------------------------------------------------------



INSERT INTO [dbo].[JobCodes] ([Id],[Title],[Description],[LabourCode],[CreateDate],[CreateBy],[UpdateDate],[UpdateBy],[IsActive])
VALUES(1,'Administrator','The administrative staff responsible for facility management such as the  administrator, assistant administrator, and other staff in the individual  departments who do not perform services described below. Do not  include the food service supervisor, housekeeping services supervisor,  or facility engineer.  ',1,getdate(),'Manual',getdate(),null,1),
(2,'Medical Director','A physician designated as responsible for implementation of resident  care policies and coordination of medical care in the facilityin accordance  with 483.75(i).',2,getdate(),'Manual',getdate(),null,1),
(3,'Other Physician','A salaried physician, other than the medical director, who supervises the  care of residents when the attending physician is unavailable, and/or a  physician(s) available to provide emergency services 24 hours a day.',2,getdate(),'Manual',getdate(),null,1),
(4,'Physician Assistant','A graduate of an accredited educational program for physician  assistants who provides healthcare services typically performed by a  physician, under the supervision of a physician.',2,getdate(),'Manual',getdate(),null,1),
(5,'Registered Nurse Director of Nursing','Professional registered nurse(s) administratively responsible for  managing and supervising nursing services within the facility. Do not  additionally reflect these hours in any other category.',3,getdate(),'Manual',getdate(),null,1),
(6,'Registered Nurse with Administrative Duties','Nurses (RN) who, as either a facility employee or contractor, perform  the Resident Assessment Instrument function in the facility and do not  perform direct care functions. Also include other RNs whose principal  duties are spent conducting administrative functions. For example, the  Assistant Director of Nursing is conducting educational/in-service, or  other duties which are not considered to be direct care giving. Facilities  with an RN waiver who do not have an RN as DON report all  administrative nursing hours in this category.',3,getdate(),'Manual',getdate(),null,1),
(7,'Registered Nurse','Those persons licensed to practice as registered nurses in the State  where the facility is located. Includes geriatric nurse practitioners and  clinical nurse specialists who primarily perform nursing, not  physician-delegated tasks. Do not include Registered Nurses hours  reported elsewhere.',3,getdate(),'Manual',getdate(),null,1),
(8,'Licensed Practical/Vocational Nurse with Administrative Duties','Those persons licensed to practice as licensed practical/vocational  nurses in the State where the facility is located, and do not perform  direct care functions. Also include other nurses whose principal duties  are spent conducting administrative functions. For example, the LPN  Charge Nurse is conducting educational/in-service, or other duties  which are not considered to be direct care giving. ',3,getdate(),'Manual',getdate(),null,1),
(9,'Licensed Practical/Vocational Nurse','Those persons licensed to practice as licensed practical/vocational  nurses in the State where the facility is located. Do not include those  hours of LPN/LVNs reported elsewhere.',3,getdate(),'Manual',getdate(),null,1),
(10,'Certified Nurse Aide','Individuals who have completed a State approved training and  competency evaluation program, or competency evaluation program  approved by the State, or have been determined competent as provided  in 483.150(a) and (3) and who are providing nursing or nursing-related  services to residents. Do not include volunteers.',3,getdate(),'Manual',getdate(),null,1),
(11,'Nurse Aide in Training','Individuals who are in the first 4 months of employment and who are  receiving training in a State approved Nurse Aide training and  competency evaluation program and are providing nursing or  nursing-related services for which they have been trained and are  under the supervision of a licensed or registered nurse. Do not include  volunteers.',3,getdate(),'Manual',getdate(),null,1),
(12,'MedicationAide/Technician','Individuals, other than a licensed professional, who fulfill the State  requirement for approval to administer medications to residents.',3,getdate(),'Manual',getdate(),null,1),
(13,'Nurse Practitioner','A registered nurse with specialized graduate education who is licensed  by the state to diagnose and treat illness, independently or as part of a  healthcare team.',3,getdate(),'Manual',getdate(),null,1),
(14,'Clinical Nurse Specialist','A registered nurse with specialized graduate education who provides  advanced nursing care.',3,getdate(),'Manual',
getdate(),null,1),
(15,'Pharmacist','The licensed pharmacist(s) who a facility is required to use for various  purposes, including providing consultation on pharmacy services,  establishing a system of records of controlled drugs, overseeing records  and reconciling controlled drugs, and/or performing a monthly drug  regimen review for each resident.',4,getdate(),'Manual',getdate(),null,1),
(16,'Dietitian','A person(s), employed full, part-time or on a consultant basis, who is  either registered by the Commission of Dietetic Registration of the  American Dietetic Association, or is qualified to be a dietitian on the  basis of experience in identification of dietary needs, planning and  implementation of dietary programs.',5,getdate(),'Manual',getdate(),null,1),
(17,'Food Service Worker','Persons (excluding the dietitian) who carry out the functions of the  dietary service (e.g., prepare and cook food, serve food, wash dishes).  Includes the food services supervisor.',5,getdate(),'Manual',getdate(),null,1),
(18,'OccupationalTherapist','Persons licensed/registered as occupational therapists according to State  law in the State in which the facility is located. Include OTs who spend  less than 50 percent of their time as activities therapists.',6,getdate(),'Manual',getdate(),null,1),
(19,'Occupational Therapy Assistant','Person(s) who, in accord with State law, have licenses/certification and  specialized training to assist a licensed/certified/registered Occupational  Therapist (OT) to carry out the OTs comprehensive plan of care, without  the direct supervision of the therapist. Include OT Assistants who spend  less than 50 percent of their time as Activities Therapists.',6,getdate(),'Manual',getdate(),null,1),
(20,'Occupational Therapy Aide','Person(s) who have specialized training to assist an OT to carry out the  OTs comprehensive plan of care under the direct supervision of the  therapist, in accord with State law.',6,getdate(),'Manual',getdate(),null,1),
(21,'Physical Therapist','Persons licensed/registered as physical therapists, according to State  law where the facility is located.',6,getdate(),'Manual',getdate(),null,1),
(22,'Physical Therapy Assistant','Person(s) who, in accord with State law, have licenses/certification and  specialized training to assist a licensed/certified/registered Physical  Therapist (PT) to carry out the PTs comprehensive plan of care, without  the direct supervision of the PT.',6,getdate(),'Manual',getdate(),null,1),
(23,'Physical Therapy Aide','Person(s) who have specialized training to assist a PT to carry out the  PTs comprehensive plan of care under the direct supervision of the  therapist, in accordance with State law.',6,getdate(),'Manual',getdate(),null,1),
(24,'Respiratory Therapist','Persons(s) who are licensed under state law (except in Alaska) as  respiratory therapists.',6,getdate(),'Manual',getdate(),null,1),
(25,'Respiratory Therapy Technician','Person(s) who provide respiratory care under the direction of  respiratory therapists and physicians.',6,getdate(),'Manual',getdate(),null,1),
(26,'Speech/LanguagePathologist','Persons licensed/registered, according to State law where the facility is  located, to provide speech therapy and related services (e.g., teaching a  resident to swallow).',6,getdate(),'Manual',getdate(),null,1),
(27,'Therapeutic Recreation Specialist','Person(s) who, in accordance with State law, are licensed/registered and  are eligible for certification as a therapeutic recreation specialist by a  recognized accrediting body.',6,getdate(),'Manual',getdate(),null,1),
(28,'Qualified Activities Professional','Person(s) who meet the definition of activities professional at  483.15(f)(2)(i)(A) and (B) or 483.15(f)(2)(ii) or (iii) or (iv) and who are  providing an on-going program of activities designed to meet residents  interests and physical, mental or psychosocial needs. Do not include  hours reported as Therapeutic Recreation Specialist, Occupational  Therapist, OT Assistant, or other categories listed above.',6,getdate(),'Manual',getdate(),null,1),
(29,'Other Activities Staff','Persons providing an on-going program of activities designed to meet  residents needs and interests. Do not include volunteers or hours  reported elsewhere.',6,getdate(),'Manual',getdate(),null,1),
(30,'Qualified Social Worker','Person licensed to practice social work in the State where the facility is  located, or if licensure is not required, persons with a bachelors degree  in social work, a bachelors degree in a human services field including  but not limited to sociology, special education, rehabilitation counseling  and psychology, and one year of supervised social work experience in a  health care setting working directly with elderly individuals.',6,getdate(),'Manual',getdate(),null,1),
(31,'Other Social Worker','Person(s) other than the qualified social worker who are involved in  providing medical social services to residents. Do not include volunteers.',6,getdate(),'Manual',getdate(),null,1),
(32,'Dentist','Persons licensed as dentists, according to State law where the facility is  located, to provide routine and emergency dental services.',7,getdate(),'Manual',getdate(),null,1),
(33,'Podiatrist','Persons licensed/registered as podiatrists, according to State law where  the facility is located, to provide podiatric care.',8,getdate(),'Manual',getdate(),null,1),
(34,'Mental Health Service Worker','Staff (excluding those included under therapeutic services) who provide  programs of services targeted to residents mental, emotional,  psychological, or psychiatric well-being and which are intended to:  � Diagnose, describe, or evaluate a residents mental or emotional  status;  � Prevent deviations from mental or emotional well-being from  developing; or  � Treat the resident according to a planned regimen to assist him/her in  regaining, maintaining, or increasing emotional abilities to function.  Among the specific services included are psychotherapy and counseling,  and administration and monitoring of psychotropic medications  targeted to a psychiatric diagnosis.',9,getdate(),'Manual',getdate(),null,1),
(35,'Vocational Service Worker','Evaluation and training aimed at assisting the resident to enter, re-enter,  or maintain employment in the labor force, including training for jobs in  integrated settings (i.e., those which have both disabled and nondisabled  workers) as well as in special settings such as sheltered workshops.',10,getdate(),'Manual',getdate(),null,1),
(36,'Clinical Laboratory Service Worker','Entities that provide laboratory services and are approved by Medicare  as independent laboratories or hospitals.',11,getdate(),'Manual',getdate(),null,1),
(37,'Diagnostic X-ray Service Worker','Radiology services, ordered by a physician, for diagnosis of a disease or  other medical condition.',12,getdate(),'Manual',getdate(),null,1),
(38,'Blood Service Worker','Blood bank and transfusion services.',13,getdate(),'Manual',getdate(),null,1),
(39,'Housekeeping Service Worker','Services, including those of the maintenance department, necessary to  maintain the environment. Includes equipment kept in a clean, safe,  functioning and sanitary condition. Includes housekeeping services  supervisor and facility engineer.',14,getdate(),'Manual',getdate(),null,1),
(40,'Other Service Worker','Record total hours worked for all personnel not already recorded (For  example, librarian).',15,getdate(),'Manual',getdate(),null,1)




