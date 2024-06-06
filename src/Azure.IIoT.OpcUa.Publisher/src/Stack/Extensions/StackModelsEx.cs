// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Publisher.Stack
{
    using Azure.IIoT.OpcUa.Publisher.Models;
    using Azure.IIoT.OpcUa.Encoders;
    using Furly.Extensions.Serializers;
    using Opc.Ua;
    using Opc.Ua.Extensions;
    using DiagnosticsLevel = Publisher.Models.DiagnosticsLevel;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Stack models extensions
    /// </summary>
    public static class StackModelsEx
    {
        /// <summary>
        /// Convert diagnostics to request header
        /// </summary>
        /// <param name="header"></param>
        /// <param name="timeoutHint"></param>
        /// <returns></returns>
        public static RequestHeader ToRequestHeader(this RequestHeaderModel? header,
            uint timeoutHint = 0)
        {
            return (header?.Diagnostics?.Level).ToRequestHeader(header?.Diagnostics?.AuditId,
                header?.Diagnostics?.TimeStamp, timeoutHint);
        }

        /// <summary>
        /// Convert diagnostics to request header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="level"></param>
        /// <param name="timestamp"></param>
        /// <param name="timeoutHint"></param>
        /// <returns></returns>
        public static RequestHeader ToRequestHeader(this OperationContextModel? context,
            DiagnosticsLevel? level = null, DateTime? timestamp = null,
            uint timeoutHint = 0)
        {
            return level.ToRequestHeader(context?.AuthorityId, timestamp, timeoutHint);
        }

        /// <summary>
        /// Convert diagnostics to request header
        /// </summary>
        /// <param name="level"></param>
        /// <param name="auditId"></param>
        /// <param name="timestamp"></param>
        /// <param name="timeoutHint"></param>
        /// <returns></returns>
        public static RequestHeader ToRequestHeader(this DiagnosticsLevel? level,
            string? auditId = null, DateTime? timestamp = null, uint timeoutHint = 0)
        {
            return new RequestHeader
            {
                AuditEntryId = auditId ?? Guid.NewGuid().ToString(),
                ReturnDiagnostics =
                    (uint)(level ?? DiagnosticsLevel.Status)
                     .ToStackType(),
                Timestamp = timestamp ?? DateTime.UtcNow,
                TimeoutHint = timeoutHint,
                AdditionalHeader = null // TODO
            };
        }

        /// <summary>
        /// Convert diagnostics to request header
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(model))]
        public static ViewDescription? ToStackModel(this BrowseViewModel? model,
            IServiceMessageContext context)
        {
            if (model is null)
            {
                return null;
            }
            return new ViewDescription
            {
                Timestamp = model.Timestamp ?? DateTime.MinValue,
                ViewVersion = model.Version ?? 0,
                ViewId = model.ViewId.ToNodeId(context)
            };
        }

        /// <summary>
        /// Convert role permission type to service model
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="namespaceFormat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static RolePermissionModel ToServiceModel(this RolePermissionType type,
            IServiceMessageContext context, NamespaceFormat namespaceFormat)
        {
            var roleId = type.RoleId.AsString(context, namespaceFormat);
            if (roleId == null)
            {
                throw new ArgumentException("Permission type not a valid node id");
            }
            return new RolePermissionModel
            {
                RoleId = roleId,
                Permissions = ((PermissionType)type.Permissions).ToServiceType()
            };
        }

        /// <summary>
        /// Convert to stack model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(model))]
        public static DataChangeFilter? ToStackModel(this DataChangeFilterModel? model)
        {
            if (model is null)
            {
                return null;
            }
            return new DataChangeFilter
            {
                DeadbandValue = model.DeadbandValue ?? 0.0,
                DeadbandType = (uint)model.DeadbandType.ToStackType(),
                Trigger = model.DataChangeTrigger.ToStackType()
            };
        }

        /// <summary>
        /// Convert to stack model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(model))]
        public static AggregateFilter? ToStackModel(this AggregateFilterModel? model,
            IServiceMessageContext context)
        {
            if (model is null)
            {
                return null;
            }
            return new AggregateFilter
            {
                AggregateConfiguration = model.AggregateConfiguration.ToStackModel(),
                AggregateType = model.AggregateTypeId.ToNodeId(context),
                StartTime = model.StartTime ?? DateTime.MinValue,
                ProcessingInterval = (model.ProcessingInterval?.TotalMilliseconds) ?? 0.0
            };
        }

        /// <summary>
        /// Convert to stack model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AggregateConfiguration ToStackModel(
            this AggregateConfigurationModel? model)
        {
            if (model is null)
            {
                return new AggregateConfiguration
                {
                    UseServerCapabilitiesDefaults = true
                };
            }
            return new AggregateConfiguration
            {
                PercentDataBad = model.PercentDataBad ?? 0,
                PercentDataGood = model.PercentDataGood ?? 0,
                TreatUncertainAsBad = model.TreatUncertainAsBad ?? true,
                UseSlopedExtrapolation = model.UseSlopedExtrapolation ?? true
            };
        }

        /// <summary>
        /// Convert to stack model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(model))]
        public static SimpleAttributeOperand? ToStackModel(this SimpleAttributeOperandModel? model,
            IServiceMessageContext context)
        {
            if (model is null)
            {
                return null;
            }
            return new SimpleAttributeOperand
            {
                TypeDefinitionId = model.TypeDefinitionId.ToNodeId(context),
                AttributeId = (uint)(model.AttributeId ?? NodeAttribute.Value),
                BrowsePath = new QualifiedNameCollection(model.BrowsePath == null ?
                    Enumerable.Empty<QualifiedName>() :
                    model.BrowsePath.Select(n => n.ToQualifiedName(context))),
                IndexRange = model.IndexRange
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <param name="namespaceFormat"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(model))]
        public static SimpleAttributeOperandModel? ToServiceModel(this SimpleAttributeOperand? model,
            IServiceMessageContext context, NamespaceFormat namespaceFormat)
        {
            if (model is null)
            {
                return null;
            }
            return new SimpleAttributeOperandModel
            {
                TypeDefinitionId = model.TypeDefinitionId
                    .AsString(context, namespaceFormat),
                AttributeId = (NodeAttribute)model.AttributeId,
                BrowsePath = model.BrowsePath?
                    .Select(p => p.AsString(context, namespaceFormat))
                    .ToArray(),
                IndexRange = model.IndexRange
            };
        }

        /// <summary>
        /// Convert user token policies to service model
        /// </summary>
        /// <param name="policies"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static IReadOnlyList<AuthenticationMethodModel> ToServiceModel(
            this UserTokenPolicyCollection policies, IJsonSerializer serializer)
        {
            if (policies == null || policies.Count == 0)
            {
                return new List<AuthenticationMethodModel>
                {
                     new() {
                         Id = "Anonymous",
                         CredentialType = CredentialType.None
                     }
                };
            }
            return policies
                .Select(p => p.ToServiceModel(serializer)!)
                .Where(p => p != null)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Convert user token policy to service model
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static AuthenticationMethodModel? ToServiceModel(
            this UserTokenPolicy? policy, IJsonSerializer serializer)
        {
            if (policy == null)
            {
                return null;
            }
            var configuration = VariantValue.Null;
            var credentialType = CredentialType.None;
            switch (policy.TokenType)
            {
                case UserTokenType.Anonymous:
                    break;
                case UserTokenType.UserName:
                    credentialType = CredentialType.UserName;
                    break;
                case UserTokenType.Certificate:
                    credentialType = CredentialType.X509Certificate;
                    configuration = policy.IssuerEndpointUrl;
                    break;
                case UserTokenType.IssuedToken:
                    switch (policy.IssuedTokenType)
                    {
                        case "http://opcfoundation.org/UA/UserToken#JWT":
                            credentialType = CredentialType.JwtToken;
                            try
                            {
                                // See part 6
                                configuration = serializer.Parse(policy.IssuerEndpointUrl);
                            }
                            catch
                            {
                                // Store as string
                                configuration = policy.IssuerEndpointUrl;
                            }
                            break;
                        default:
                            // TODO
                            return null;
                    }
                    break;
                default:
                    return null;
            }
            return new AuthenticationMethodModel
            {
                Id = policy.PolicyId,
                SecurityPolicy = policy.SecurityPolicyUri,
                Configuration = configuration,
                CredentialType = credentialType
            };
        }

        /// <summary>
        /// Makes a user identity
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="ServiceResultException"></exception>
        public static async ValueTask<IUserIdentity> ToUserIdentityAsync(
            this CredentialModel? credential, ApplicationConfiguration configuration)
        {
            if (credential == null || credential.Type == CredentialType.None)
            {
                return new UserIdentity(new AnonymousIdentityToken());
            }
            var identity = credential.Value;
            if (identity == null)
            {
                throw new ServiceResultException(StatusCodes.BadInvalidArgument,
                    $"Credential type {credential.Type} requires providing a credential value.");
            }
            switch (credential.Type)
            {
                case CredentialType.UserName:
                    return new UserIdentity(identity.User, identity.Password);
                case CredentialType.X509Certificate:
                    var subjectName = identity.User;
                    var thumbprint = identity.Thumbprint;
                    var passCode = identity.Password;
                    if (thumbprint != null || subjectName != null)
                    {
                        using var users = configuration.SecurityConfiguration
                            .TrustedUserCertificates.OpenStore();
                        var userCertWithPrivateKey = await users.LoadPrivateKey(
                            thumbprint, subjectName, passCode).ConfigureAwait(false);
                        if (userCertWithPrivateKey == null)
                        {
                            throw new ServiceResultException(StatusCodes.BadCertificateInvalid,
                                $"User certificate for {subjectName ?? thumbprint} missing " +
                                "or provided password invalid. Please configure the User " +
                                "Certificate correctly in the User certificate store.");
                        }
                        return new UserIdentity(userCertWithPrivateKey);
                    }
                    throw new ServiceResultException(StatusCodes.BadNotSupported,
                       "X509Certificate credential requires to set either a thumbprint or subject name (user).");
                case CredentialType.None:
                    return new UserIdentity(new AnonymousIdentityToken());
                default:
                    throw new ServiceResultException(StatusCodes.BadNotSupported,
                        $"Credential type {credential!.Type} is not supported");
            }
        }

        /// <summary>
        /// Convert to stack model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataSetMetaDataType ToStackModel(
            this PublishedDataSetMetaDataModel model, IServiceMessageContext context)
        {
            return new DataSetMetaDataType
            {
                Name = model.DataSetMetaData.Name,
                Description = model.DataSetMetaData.Description,
                DataSetClassId = (Uuid)model.DataSetMetaData.DataSetClassId,
                Namespaces = context.NamespaceUris.ToArray(),
                StructureDataTypes = model.StructureDataTypes?
                    .Select(s => s.ToStackModel(context)).ToArray(),
                EnumDataTypes = model.EnumDataTypes?
                    .Select(e => e.ToStackModel(context)).ToArray(),
                SimpleDataTypes = model.SimpleDataTypes?
                    .Select(e => e.ToStackModel(context)).ToArray(),
                Fields = model.Fields
                    .Select(e => e.ToStackModel(context)).ToArray(),
                ConfigurationVersion = new ConfigurationVersionDataType
                {
                    MajorVersion = model.DataSetMetaData.MajorVersion ?? 1,
                    MinorVersion = model.MinorVersion
                }
            };
        }

        /// <summary>
        /// Convert to type description
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static EnumDescription ToStackModel(this EnumDescriptionModel model,
            IServiceMessageContext context)
        {
            return new EnumDescription
            {
                BuiltInType = model.BuiltInType ?? (byte)BuiltInType.Null,
                DataTypeId = model.DataTypeId.ToNodeId(context),
                Name = model.Name,
                EnumDefinition = new EnumDefinition
                {
                    Fields = model.Fields.Select(f => f.ToStackModel()).ToArray(),
                    IsOptionSet = model.IsOptionSet
                }
            };
        }

        /// <summary>
        /// Convert to field
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static EnumField ToStackModel(this EnumFieldDescriptionModel model)
        {
            return new EnumField
            {
                Name = model.Name,
                DisplayName = model.DisplayName,
                Value = model.Value
            };
        }

        /// <summary>
        /// Convert to type description
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static StructureDescription ToStackModel(this StructureDescriptionModel model,
            IServiceMessageContext context)
        {
            return new StructureDescription
            {
                Name = model.Name,
                DataTypeId = model.DataTypeId.ToNodeId(context),
                StructureDefinition = new StructureDefinition
                {
                    BaseDataType = model.BaseDataType.ToNodeId(context),
                    DefaultEncodingId = model.DefaultEncodingId,
                    FirstExplicitFieldIndex = 0,
                    StructureType = model.StructureType.ToStackType(),
                    Fields = model.Fields.Select(f => f.ToStackModel(context)).ToArray()
                }
            };
        }

        /// <summary>
        /// Convert to field
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static StructureField ToStackModel(this StructureFieldDescriptionModel model,
            IServiceMessageContext context)
        {
            return new StructureField
            {
                Name = model.Name,
                ArrayDimensions = model.ArrayDimensions?.ToArray(),
                IsOptional = model.IsOptional,
                DataType = model.DataType.ToNodeId(context),
                Description = model.Description,
                MaxStringLength = model.MaxStringLength,
                ValueRank = model.ValueRank
            };
        }

        /// <summary>
        /// Convert to type description
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static SimpleTypeDescription ToStackModel(this SimpleTypeDescriptionModel model,
            IServiceMessageContext context)
        {
            return new SimpleTypeDescription
            {
                BaseDataType = model.BaseDataType.ToNodeId(context),
                BuiltInType = model.BuiltInType ?? (byte)BuiltInType.Null,
                Name = model.Name,
                DataTypeId = model.DataTypeId.ToNodeId(context)
            };
        }

        /// <summary>
        /// Convert to field metadata
        /// </summary>
        /// <param name="field"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static FieldMetaData ToStackModel(this PublishedFieldMetaDataModel field,
            IServiceMessageContext context)
        {
            return new FieldMetaData
            {
                Name = field.Name,
                DataSetFieldId = (Uuid)field.Id,
                ArrayDimensions = field.ArrayDimensions?.ToArray(),
                BuiltInType = field.BuiltInType,
                DataType = field.DataType.ToNodeId(context),
                Description = field.Description,
                MaxStringLength = field.MaxStringLength,
                ValueRank = field.ValueRank,
                FieldFlags = field.Flags,
                Properties = null // TODO
            };
        }
    }
}
