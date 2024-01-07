using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Resources;
using TANWeb.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TANWeb.Areas.LabourFileMover.Controllers
{
    [Area("LabourFileMover")]
    public class KronosOrgCredsController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        #region Constructor
        public KronosOrgCredsController(IUnitOfWork uow, UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        #endregion
        #region List View of existing Org Creds
        [Route("kronos/credentials")]
        public async Task<IActionResult> Index()
        {
            KronosFormViewModel kronosFormView = new KronosFormViewModel();
            List<KronosOrgVM> KorgsList = new List<KronosOrgVM>();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Other))
            {
                var userOrg = _uow.UserOrganizationFacilitiesRepo.GetAll().FirstOrDefault(c => c.UserId == currentUser.Id);
                if (userOrg != null)
                {
                    kronosFormView.OrgList = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == userOrg.OrganizationID && x.IsActive).Select(x => new Itemlist { Text = x.OrganizationName, Value = x.OrganizationID }).ToList();
                    var credsList = _uow.KronosSftpCredsRepo.GetAll().Where(x => x.OrganizationId == userOrg.OrganizationID).ToList();
                    foreach (var item in credsList)
                    {
                        KronosOrgVM Korgs = new KronosOrgVM();
                        var org = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == item.OrganizationId && x.IsActive).FirstOrDefault();
                        Korgs.OrgName = org.OrganizationName;
                        Korgs.OrgId = org.OrganizationID;
                        var credsOn = _uow.KronosSftpCredsRepo.GetAll().Where(_ => _.OrganizationId == item.OrganizationId).FirstOrDefault();
                        Korgs.IsActive = credsOn!=null? credsOn.IsActive :false;

                        Korgs.FacilityCount = _uow.FacilityRepo.GetAll().Where(c => c.OrganizationId == item.OrganizationId).Count();
                        KorgsList.Add(Korgs);

                    }
                }
            }
            else
            {
                kronosFormView.OrgList = _uow.OrganizationRepo.GetAll().Where(x=> x.IsActive).Select(x => new Itemlist { Text = x.OrganizationName, Value = x.OrganizationID }).ToList();
                var credsList = _uow.KronosSftpCredsRepo.GetAll().ToList();
                foreach (var item in credsList)
                {
                    KronosOrgVM Korgs = new KronosOrgVM();
                    var org = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == item.OrganizationId && x.IsActive).FirstOrDefault();
                    Korgs.OrgName = org.OrganizationName;
                    Korgs.OrgId = org.OrganizationID;
                    var credsOn = _uow.KronosSftpCredsRepo.GetAll().Where(_ => _.OrganizationId == item.OrganizationId).FirstOrDefault();
                    Korgs.IsActive = credsOn != null ? credsOn.IsActive : false;
                    Korgs.FacilityCount = _uow.FacilityRepo.GetAll().Where(c => c.OrganizationId == item.OrganizationId).Count();
                    KorgsList.Add(Korgs);
                }
            }
            kronosFormView.KronosOrgList = KorgsList;
            return View(kronosFormView);
        }
        #endregion   
        public async Task<IActionResult> CheckOrgCredentials(string id)
        {
            var creds = _uow.KronosSftpCredsRepo.GetAll().Where(x => x.OrganizationId == Convert.ToInt32(id)).FirstOrDefault();
            if (creds != null)
            {
                return Json(false);
            }
            return Json(true);
        }
        #region Change Status
        public async Task<IActionResult> ChangeStatus(string userId, string status)
        {

            string response = "";
            var userStatus = true;
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    if (status == "0")
                    {
                        userStatus = false;
                    }
                    //change status
                    if (userId != null && status != null)
                    {
                        var kronosSftpCreds = _uow.KronosSftpCredsRepo.GetAll().Where(x => x.OrganizationId.ToString().Equals(userId)).FirstOrDefault();
                        var kronosFile = _uow.kronosFileEndPointRepo.GetAll().Where(x => x.OrganizationId.ToString().Equals(userId)).FirstOrDefault();
                        var KronosBlob = _uow.kronosBlobCredsRepo.GetAll().Where(c => c.OrganizationId.ToString().Equals(userId)).FirstOrDefault();
                        if (kronosSftpCreds != null)
                        {
                            kronosSftpCreds.IsActive = Convert.ToBoolean(userStatus);
                            _uow.KronosSftpCredsRepo.Update(kronosSftpCreds);
                            _uow.SaveChanges();
                            response = OperationResult.Success.ToString();
                        }
                        else { response = ""; }
                        if (kronosFile != null)
                        {
                            kronosFile.IsActive = Convert.ToBoolean(userStatus);
                            _uow.kronosFileEndPointRepo.Update(kronosFile);
                            _uow.SaveChanges();
                            response = OperationResult.Success.ToString();
                        }
                        else { response = ""; }
                        if (KronosBlob != null)
                        {
                            KronosBlob.IsActive = Convert.ToBoolean(userStatus);
                            _uow.kronosBlobCredsRepo.Update(KronosBlob);
                            _uow.SaveChanges();
                            response = OperationResult.Success.ToString();
                        }
                        else { response = ""; }
                        if (response == OperationResult.Success.ToString())
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = OperationResult.Error.ToString();
                    transaction.Rollback();
                    Log.Error(ex, "An Error Occured in User Controller :{ErrorMsg}", ex.Message);
                }
            }
            return Json(response);
        }
        #endregion
        #region Insert data of the CombinedForm
        public async Task<IActionResult> AddCombinedData(KronosFormViewModel formViewModel)
        {
            var result = "";
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    //sftp
                    KronosSftpCredentials cred = new KronosSftpCredentials();
                    cred.KronosHostGLC = formViewModel.KronosHostGLC;
                    cred.KronosPortGLC = formViewModel.KronosPortGLC;
                    cred.KronosPasswordGLC = formViewModel.KronosPasswordGLC;
                    cred.KronosUserNameGLC = formViewModel.KronosUserNameGLC;
                    cred.OrganizationId = Convert.ToInt32(formViewModel.OrganizationId);
                    //file
                    KronosSftpFileEndpoints file = new KronosSftpFileEndpoints();
                    file.KronosPunchExportGLC = formViewModel.KronosPunchExportGLC;
                    file.OrganizationId = Convert.ToInt32(formViewModel.OrganizationId);
                    //blob
                    KronosBlobStorageCreds blob = new KronosBlobStorageCreds();
                    blob.StorageConnectionString = formViewModel.StorageConnectionString;
                    blob.KronosDecryptOrEncryptKeyBlobNameGLC = formViewModel.KronosDecryptOrEncryptKeyBlobNameGLC;
                    blob.KronosDecryptionPasswordGLC = formViewModel.KronosDecryptionPasswordGLC;
                    blob.OrganizationId = Convert.ToInt32(formViewModel.OrganizationId);
                    //call AddSftPCreds
                    var operations = new Func<Task<string>>[]
                    {
                        async () =>
                        {
                            var sftpResult = await AddSftpCreds(cred);
                            return sftpResult.Value.ToString();
                        },
                        async () =>
                        {
                            var fileResult = await AddSftpFileEndPoints(file);
                            return fileResult.Value.ToString();
                        },
                        async () =>
                        {
                            var blobCredResult = await AddBlobCreds(blob);
                            return blobCredResult.Value.ToString();
                        }
                     };

                    var results = await Task.WhenAll(operations.Select(op => op()));
                    foreach (var individualResult in results)
                    {
                        if (individualResult != OperationResult.Success.ToString())
                        {
                            result = individualResult; // Update overall result if any individual operation fails
                            break; // Stop further checking if any operation has failed
                        }
                        else
                        {
                            result = OperationResult.Success.ToString();
                        }
                    }

                    if (result == OperationResult.Success.ToString())
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }

                    // Log or handle the exception appropriately               

                }
                catch (Exception ex)
                {
                    result = OperationResult.Failed.ToString();
                    transaction.Rollback();
                }
            }
            return Json(result);
        }
        #endregion
        #region AddSftPCreds
        public async Task<JsonResult> AddSftpCreds(KronosSftpCredentials sftpCreds)
        {
            KronosSftpCredentials kronosSftpcreds = new KronosSftpCredentials();
            string result = "";
            try
            {
                var ExistingOrg = _uow.KronosSftpCredsRepo.GetAll().Where(_x => _x.OrganizationId == sftpCreds.OrganizationId).FirstOrDefault();
                if (ExistingOrg == null)
                {
                    kronosSftpcreds.KronosHostGLC = EncryptionService.Encrypt(sftpCreds.KronosHostGLC);
                    kronosSftpcreds.KronosPortGLC = EncryptionService.Encrypt(sftpCreds.KronosPortGLC);
                    kronosSftpcreds.KronosUserNameGLC = EncryptionService.Encrypt(sftpCreds.KronosUserNameGLC);
                    kronosSftpcreds.KronosPasswordGLC = EncryptionService.Encrypt(sftpCreds.KronosPasswordGLC);
                    kronosSftpcreds.OrganizationId = Convert.ToInt32(sftpCreds.OrganizationId);
                    _uow.KronosSftpCredsRepo.Add(kronosSftpcreds);
                    _uow.SaveChanges();
                    TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                    result = OperationResult.Success.ToString();
                    return Json(result);
                }
                else
                {
                    result = OperationResult.DataExists.ToString();
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                result = OperationResult.Failed.ToString();
                return Json(result);
            }
        }
        #endregion
        #region AddBlobCreds
        public async Task<JsonResult> AddBlobCreds(KronosBlobStorageCreds blobCreds)
        {
            KronosBlobStorageCreds kronosBlobStorage = new KronosBlobStorageCreds();
            var result = "";
            try
            {
                var ExistingOrg = _uow.kronosBlobCredsRepo.GetAll().Where(_x => _x.OrganizationId == blobCreds.OrganizationId).FirstOrDefault();
                if (ExistingOrg == null)
                {
                    kronosBlobStorage.StorageConnectionString = EncryptionService.Encrypt(blobCreds.StorageConnectionString);
                    kronosBlobStorage.KronosDecryptionPasswordGLC = EncryptionService.Encrypt(blobCreds.KronosDecryptionPasswordGLC);
                    kronosBlobStorage.KronosDecryptOrEncryptKeyBlobNameGLC = EncryptionService.Encrypt(blobCreds.KronosDecryptOrEncryptKeyBlobNameGLC);
                    kronosBlobStorage.OrganizationId = Convert.ToInt32(blobCreds.OrganizationId);
                    _uow.kronosBlobCredsRepo.Add(kronosBlobStorage);
                    _uow.SaveChanges();
                    TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                    result = OperationResult.Success.ToString();
                    return Json(result);
                }
                else
                {
                    result = OperationResult.DataExists.ToString();
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                result =OperationResult.Failed.ToString();
                return Json(result);
            }
        }
        #endregion
        #region AddSftpFileEndPoints
        public async Task<JsonResult> AddSftpFileEndPoints(KronosSftpFileEndpoints blobCreds)
        {
            KronosSftpFileEndpoints kronosSftpFile = new KronosSftpFileEndpoints();
            var result = "";
            try
            {
                var ExistingOrg = _uow.kronosFileEndPointRepo.GetAll().Where(_x => _x.OrganizationId == blobCreds.OrganizationId).FirstOrDefault();
                if (ExistingOrg == null)
                {
                    kronosSftpFile.KronosPunchExportGLC = EncryptionService.Encrypt(blobCreds.KronosPunchExportGLC);
                    kronosSftpFile.OrganizationId = Convert.ToInt32(blobCreds.OrganizationId.ToString());
                    kronosSftpFile.KronosPayrollGLC = EncryptionService.Encrypt("NA");
                    _uow.kronosFileEndPointRepo.Add(kronosSftpFile);
                    _uow.SaveChanges();
                    TempData["SuccessmsgPop"] = TANResource.SuccessMsg.ToString();
                    result = OperationResult.Success.ToString();
                    return Json(result);
                }
                else
                {
                    result = OperationResult.DataExists.ToString();
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                result = OperationResult.Failed.ToString();
                return Json(result);
            }
        }
        #endregion
        #region Update Credentials to db

        [HttpPost]
        public async Task<JsonResult> UpdateKronos(KronosFormViewModel formVM)
        {
            var result = "";
            if (formVM != null)
            {
                using (var transaction = _uow.BeginTransaction())
                {
                    try
                    {
                        KronosSftpCredentials sftp = _uow.KronosSftpCredsRepo.GetAll().Where(c => c.OrganizationId.ToString() == formVM.OrganizationId).FirstOrDefault();
                        if (sftp != null)
                        {
                            sftp.KronosPortGLC = EncryptionService.Encrypt(formVM.KronosPortGLC);
                            sftp.KronosHostGLC = EncryptionService.Encrypt(formVM.KronosHostGLC);
                            sftp.KronosUserNameGLC = EncryptionService.Encrypt(formVM.KronosUserNameGLC);
                            sftp.KronosPasswordGLC = EncryptionService.Encrypt(formVM.KronosPasswordGLC);
                            sftp.OrganizationId = Convert.ToInt32(formVM.OrganizationId);
                            _uow.KronosSftpCredsRepo.Update(sftp);
                        }
                        //file
                        KronosSftpFileEndpoints file = _uow.kronosFileEndPointRepo.GetAll().Where(c => c.OrganizationId.ToString() == formVM.OrganizationId).FirstOrDefault();
                        if (file != null)
                        {
                            file.KronosPunchExportGLC = EncryptionService.Encrypt(formVM.KronosPunchExportGLC);
                            file.KronosPayrollGLC = "NA";
                            file.OrganizationId = Convert.ToInt32(formVM.OrganizationId);
                            _uow.kronosFileEndPointRepo.Update(file);
                        }
                        //Blob
                        KronosBlobStorageCreds blob = _uow.kronosBlobCredsRepo.GetAll().Where(c => c.OrganizationId.ToString() == formVM.OrganizationId).FirstOrDefault();
                        if (blob != null)
                        {
                            blob.KronosDecryptionPasswordGLC = EncryptionService.Encrypt(formVM.KronosDecryptionPasswordGLC);
                            blob.KronosDecryptOrEncryptKeyBlobNameGLC = EncryptionService.Encrypt(formVM.KronosDecryptOrEncryptKeyBlobNameGLC);
                            blob.StorageConnectionString = EncryptionService.Encrypt(formVM.StorageConnectionString);
                            blob.OrganizationId = Convert.ToInt32(formVM.OrganizationId);
                            _uow.kronosBlobCredsRepo.Update(blob);
                        }
                        if (blob == null && file == null && sftp == null)
                        {
                            result = OperationResult.NoDataExists.ToString();
                            return Json(result);

                        }
                        else
                        {
                            result = OperationResult.Success.ToString();
                            _uow.SaveChanges();
                            transaction.Commit();
                            return Json(result);
                        }

                    }
                    catch (Exception ex)
                    {
                        result = OperationResult.Failed.ToString();
                        transaction.Rollback();
                        return Json(result);
                    }

                }
            }
            return Json(result);
        }
        #endregion
        #region Get Crendential data foe Update view
        [Route("update/credentials")]
        public async Task<IActionResult> GetKronosData(string Id)
        {
            KronosFormViewModel viewModel = new KronosFormViewModel();
            var SftpData = _uow.KronosSftpCredsRepo.GetAll().Where(x => x.OrganizationId.ToString() == Id).FirstOrDefault();
            if (SftpData != null)
            {
                viewModel.KronosHostGLC = EncryptionService.Decrypt(SftpData.KronosHostGLC);
                viewModel.KronosPortGLC = EncryptionService.Decrypt(SftpData.KronosPortGLC);
                viewModel.KronosUserNameGLC = EncryptionService.Decrypt(SftpData.KronosUserNameGLC);
                viewModel.KronosPasswordGLC = EncryptionService.Decrypt(SftpData.KronosPasswordGLC);
                viewModel.OrganizationId = Id;
                var File = _uow.kronosFileEndPointRepo.GetAll().Where(x => x.OrganizationId.ToString() == Id).FirstOrDefault();
                if (File != null)
                {
                    viewModel.KronosPunchExportGLC = EncryptionService.Decrypt(File.KronosPunchExportGLC);

                }
                var blob = _uow.kronosBlobCredsRepo.GetAll().Where(x => x.OrganizationId.ToString() == Id).FirstOrDefault();
                if (blob != null)
                {
                    viewModel.KronosDecryptOrEncryptKeyBlobNameGLC = EncryptionService.Decrypt(blob.KronosDecryptOrEncryptKeyBlobNameGLC);
                    viewModel.KronosDecryptionPasswordGLC = EncryptionService.Decrypt(blob.KronosDecryptionPasswordGLC);
                    viewModel.StorageConnectionString = EncryptionService.Decrypt(blob.StorageConnectionString);
                }
            }
            var x = _uow.OrganizationRepo.GetAll().Select(x => new Itemlist { Text = x.OrganizationName, Value = x.OrganizationID }).ToList();
            viewModel.OrgList = x;
            ViewBag.Org = Id;
            return View(viewModel);
        }
        #endregion



    }
}
