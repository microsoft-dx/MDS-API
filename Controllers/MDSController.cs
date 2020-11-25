using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasterData.MDSService;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;

namespace MDS_REST_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MDSController : ControllerBase
    {

        private readonly ILogger<MDSController> _logger;
        
        private readonly IConfiguration _configuration; 

        private readonly ServiceClient _clientProxy;

        public MDSController(ILogger<MDSController> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._clientProxy = GetClientProxy(
                    configuration["Mds:ServiceUrl"],
                    configuration["Mds:Credentials:Domain"],
                    configuration["Mds:Credentials:Username"],
                    configuration["Mds:Credentials:Password"]                    
                );
        }

        [HttpGet]
        [Route("SayHello")]
        public string SayHello() {
            return "Hello MDS!";
        }

        // Create an entity member with a specified name, code, and member type.
        // HierarchyName is used only when the member type is Consolidated.
        [HttpPost]
        [Route("CreateEntityMember")]
        public EntityMembersCreateResponse CreateEntityMember(string modelName, string versionName, string entityName, string aNewMemberName, string aNewCode, MemberType memberType, string hierarchyName = null, string changesetName = null)
        {
            // Create the request object for entity creation.
            EntityMembersCreateRequest createRequest = new EntityMembersCreateRequest();
            createRequest.Members = new EntityMembers();
            createRequest.ReturnCreatedIdentifiers = true;
            // Set the modelId, versionId, and entityId.
            createRequest.Members.ModelId = new Identifier { Name = modelName };
            createRequest.Members.VersionId = new Identifier { Name = versionName };
            createRequest.Members.EntityId = new Identifier { Name = entityName };
            createRequest.Members.MemberType = memberType;
            createRequest.Members.Members = new System.Collections.ObjectModel.Collection<Member> { };
            Member aNewMember = new Member();
            aNewMember.MemberId = new MemberIdentifier() { Name = aNewMemberName, Code = aNewCode, MemberType = memberType };

            if (memberType == MemberType.Consolidated)
            {
                // In case when the member type is consolidated set the parent information.
                // Set the hierarchy name and the parent code ("ROOT" means the root node of the hierarchy).
                aNewMember.Parents = new System.Collections.ObjectModel.Collection<Parent> { };
                Parent aParent = new Parent();
                aParent.HierarchyId = new Identifier() { Name = hierarchyName };
                aParent.ParentId = new MemberIdentifier() { Code = "ROOT" };
                aNewMember.Parents.Add(aParent);
            }

            if (!string.IsNullOrEmpty(changesetName))
            {
                createRequest.Members.ChangesetId = new Identifier {Name = changesetName};
            }

            createRequest.Members.Members.Add(aNewMember);

            // Create a new entity member
            EntityMembersCreateResponse createResponse = _clientProxy.EntityMembersCreate(createRequest);

            return createResponse;
        }

        [HttpPost]
        [Route("GetEntityMemberByName")]
        // Get the entity member identifier with specified model name, version name, entity name, member type, and entity member name.
        public EntityMembersGetResponse GetEntityMemberByName(string modelName, string versionName, string entityName, MemberType memberType, string memberName)
        {
            // Create the request object to get the entity information.
            EntityMembersGetRequest getRequest = new EntityMembersGetRequest();
            getRequest.MembersGetCriteria = new EntityMembersGetCriteria();
                
            // Set the modelId, versionId, entityId, and the member name.
            getRequest.MembersGetCriteria.ModelId = new Identifier { Name = modelName };
            getRequest.MembersGetCriteria.VersionId = new Identifier { Name = versionName };
            getRequest.MembersGetCriteria.EntityId = new Identifier { Name = entityName };
            getRequest.MembersGetCriteria.MemberType = memberType;
            getRequest.MembersGetCriteria.MemberReturnOption = MemberReturnOption.Data;
            getRequest.MembersGetCriteria.SearchTerm = "Name = '" + memberName + "'";

            // Get the entity member information
            EntityMembersGetResponse getResponse = _clientProxy.EntityMembersGet(getRequest);
            return getResponse;
        }

        // Get the entity member identifier with specified model name, version name, entity name, member type, and entity member code.
        [HttpPost]
        [Route("GetEntityMemberByCode")]
        public EntityMembersGetResponse GetEntityMemberByCode(string modelName, string versionName, string entityName, MemberType memberType, string memberCode)
        {
            // Create the request object to get the entity information.
            EntityMembersGetRequest getRequest = new EntityMembersGetRequest();
            getRequest.MembersGetCriteria = new EntityMembersGetCriteria();

            // Set the modelId, versionId, entityId, and the member code.
            getRequest.MembersGetCriteria.ModelId = new Identifier { Name = modelName };
            getRequest.MembersGetCriteria.VersionId = new Identifier { Name = versionName };
            getRequest.MembersGetCriteria.EntityId = new Identifier { Name = entityName };
            getRequest.MembersGetCriteria.MemberType = memberType;
            getRequest.MembersGetCriteria.MemberReturnOption = MemberReturnOption.Data;
            getRequest.MembersGetCriteria.SearchTerm = "Code = '" + memberCode + "'";

            // Get the entity member information
            EntityMembersGetResponse getResponse = _clientProxy.EntityMembersGet(getRequest);
            return getResponse;
        }


        // Update an entity member (change name) with the member code.
        [HttpPost]
        [Route("UpdateEntityMember")]
        public EntityMembersUpdateResponse UpdateEntityMember(string modelName, string versionName, string entityName, string memberCode, MemberType memberType, string newMemberName, string changesetName = null)
        {
            // Create the request object for entity update.
            EntityMembersUpdateRequest updateRequest = new EntityMembersUpdateRequest();
            updateRequest.Members = new EntityMembers();
            // Set the modelName, the versionName, and the entityName.
            updateRequest.Members.ModelId = new Identifier { Name = modelName };
            updateRequest.Members.VersionId = new Identifier { Name = versionName };
            updateRequest.Members.EntityId = new Identifier { Name = entityName };
            updateRequest.Members.MemberType = MemberType.Leaf;
            updateRequest.Members.Members = new System.Collections.ObjectModel.Collection<Member> { };
            Member aMember = new Member();
            // Set the member code.
            aMember.MemberId = new MemberIdentifier() {Code = memberCode, MemberType = memberType};
            aMember.Attributes = new System.Collections.ObjectModel.Collection<MasterData.MDSService.Attribute> { };
            // Set the new member name into the Attribute object. 
            MasterData.MDSService.Attribute anAttribute = new MasterData.MDSService.Attribute();
            anAttribute.Identifier = new Identifier() { Name = "Name" };
            anAttribute.Type = AttributeValueType.String;
            anAttribute.Value = newMemberName;
            aMember.Attributes.Add(anAttribute); 
            updateRequest.Members.Members.Add(aMember);

            if (!string.IsNullOrEmpty(changesetName))
            {
                updateRequest.Members.ChangesetId = new Identifier { Name = changesetName };
            }

            // Update the entity member (change the name).
            EntityMembersUpdateResponse updateResponse = _clientProxy.EntityMembersUpdate(updateRequest);
            return updateResponse;
        }
        
        // Update an entity member relationship.
        [HttpPost]
        [Route("UpdateEntityMemberRelationship")]
        public EntityMembersUpdateResponse UpdateEntityMemberRelationship(string modelName, string versionName, string entityName, string hierarchyName, string parentMemberCode, string childMemberCode)
        {
            // Create the request object for entity update.
            EntityMembersUpdateRequest updateRequest = new EntityMembersUpdateRequest();
            updateRequest.Members = new EntityMembers();
            // Set the modelName, the versionName, and the entityName.
            updateRequest.Members.ModelId = new Identifier { Name = modelName };
            updateRequest.Members.VersionId = new Identifier { Name = versionName };
            updateRequest.Members.EntityId = new Identifier { Name = entityName };
            updateRequest.Members.MemberType = MemberType.Leaf;
            updateRequest.Members.Members = new System.Collections.ObjectModel.Collection<Member> { };
            // Set child member information.
            Member aMember = new Member();
            aMember.MemberId = new MemberIdentifier() { Code = childMemberCode, MemberType = MemberType.Leaf };
            aMember.Attributes = new System.Collections.ObjectModel.Collection<MasterData.MDSService.Attribute> { };
            // Set parent member information.
            Parent aParent = new Parent();
            aParent.ParentId = new MemberIdentifier() { Code = parentMemberCode, MemberType = MemberType.Consolidated };
            aParent.HierarchyId = new Identifier() { Name = hierarchyName };
            aParent.RelationshipType = RelationshipType.Parent;
            aMember.Parents = new System.Collections.ObjectModel.Collection<Parent> { };
            aMember.Parents.Add(aParent);

            updateRequest.Members.Members.Add(aMember);

            // Update the entity member relationship.
            EntityMembersUpdateResponse updateResponse = _clientProxy.EntityMembersUpdate(updateRequest);
            return updateResponse;
        }

        // Delete an entity member with the member code.
        [HttpPost]
        [Route("DeleteEntityMember")]
        public EntityMembersDeleteResponse DeleteEntityMember(string modelName, string versionName, string entityName, string memberCode, MemberType memType, string changesetName = null)
        {
            // Create the request object for entity member deletion.
            EntityMembersDeleteRequest deleteRequest = new EntityMembersDeleteRequest();
            deleteRequest.Members = new EntityMembers();
            // Set the modelName, the versionName, and the entityName.
            deleteRequest.Members.ModelId = new Identifier { Name = modelName };
            deleteRequest.Members.VersionId = new Identifier { Name = versionName };
            deleteRequest.Members.EntityId = new Identifier { Name = entityName };
            deleteRequest.Members.MemberType = memType;
            deleteRequest.Members.Members = new System.Collections.ObjectModel.Collection<Member> { };
            Member aMember = new Member();
            aMember.MemberId = new MemberIdentifier() { Code = memberCode, MemberType = memType };
            deleteRequest.Members.Members.Add(aMember);

            if (!string.IsNullOrEmpty(changesetName))
            {
                deleteRequest.Members.ChangesetId = new Identifier { Name = changesetName };
            }

            // Delete the entity member.
            EntityMembersDeleteResponse deleteResponse = _clientProxy.EntityMembersDelete(deleteRequest);

            return deleteResponse;
        }

        [HttpPost]
        [Route("ChangesetSave")]
        public EntityMemberChangesetSaveResponse ChangesetSave(string modelName, string versionName, string entityName, Identifier changesetId,
            ChangesetStatus status)
        {
            var saveRequest = new EntityMemberChangesetSaveRequest
            {
                ModelId = new Identifier {Name = modelName},
                VersionId = new Identifier {Name = versionName},
                Changeset = new Changeset
                {
                    Identifier = changesetId,
                    EntityId = new Identifier {Name = entityName},
                    Status = status
                }
            };

            EntityMemberChangesetSaveResponse saveResponse = _clientProxy.EntityMemberChangesetSave(saveRequest);
            return saveResponse;
        }

        [HttpPost]
        [Route("ChangesetDelete")]
        public EntityMemberChangesetDeleteResponse ChangesetDelete(string modelName, string versionName, string name)
        {
            var deleteRequest = new EntityMemberChangesetDeleteRequest
            {
                ModelId = new Identifier { Name = modelName },
                VersionId = new Identifier { Name = versionName },
                ChangesetId = new Identifier { Name = name }
            };

            EntityMemberChangesetDeleteResponse deleteResponse = _clientProxy.EntityMemberChangesetDelete(deleteRequest);
            return deleteResponse;
        }

        [HttpPost]
        [Route("ChangesetsGet")]
        public EntityMemberChangesetsGetResponse ChangesetsGet(string modelName, string versionName, string entityName, ChangesetStatus status = ChangesetStatus.NotSpecified)
        {
            var getRequest = new EntityMemberChangesetsGetRequest
            {
                ModelId = new Identifier { Name = modelName },
                VersionId = new Identifier { Name = versionName },
                EntityId = new Identifier { Name = entityName },
                Status = status
            };

            EntityMemberChangesetsGetResponse getResponse = _clientProxy.EntityMemberChangesetsGet(getRequest);
            return getResponse;
        }



        private static ServiceClient GetClientProxy(string targetURL, string domain, string username, string password)
        {
            ServiceClient client = GetClientProxy(targetURL);
            client.ClientCredentials.Windows.ClientCredential.Domain = domain;
            client.ClientCredentials.Windows.ClientCredential.UserName = username;
            client.ClientCredentials.Windows.ClientCredential.Password = password;

            return client;
        }

        private static ServiceClient GetClientProxy(string targetURL)
        {
            // Create an endpoint address using the URL. 
            EndpointAddress endptAddress = new EndpointAddress(targetURL);

            // Create and configure the WS Http binding. 
            WSHttpBinding wsBinding = new WSHttpBinding();

            // Create and return the client proxy. 
            return new ServiceClient(wsBinding, endptAddress);
        }
    }
}
