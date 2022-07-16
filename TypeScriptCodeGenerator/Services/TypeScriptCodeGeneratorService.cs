using System.Text;
using Extensions;
using Grpc.Core;
using TypeScriptCodeGenerator.Helpers;

namespace TypeScriptCodeGenerator.Services;

public class TypeScriptCodeGeneratorService : TypeScriptCodeGenerator.TypeScriptCodeGeneratorBase
{
    private readonly ILogger<TypeScriptCodeGeneratorService> _logger;

    public TypeScriptCodeGeneratorService(ILogger<TypeScriptCodeGeneratorService> logger)
    {
        _logger = logger;
    }

    public override async Task<DtoResult> CreateDtos(Entity request, ServerCallContext context)
    {
        var result = new DtoResult();
        result.FullOutputStringify = GenerateFullOutput(request).ToString();
        result.PartOutputStringify = GeneratePartOutput(request).ToString();
        result.CreateInputStringify = GenerateCreateInput(request).ToString();
        result.UpdateInputStringify = GenerateUpdateInput(request).ToString();
        result.GetInputStringify = GenerateGetInput(request).ToString();
        result.DeleteInputStringify = GenerateDeleteInput(request).ToString();
        return result;
    }

    public override async Task<ServiceResult> CreateService(Entity request, ServerCallContext context)
    {
        var result = new ServiceResult();
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("import {API_BASE_URL} from '../../service-proxies/service-proxies';")
            .NewLine()
            .Append("import {Inject, Injectable} from '@angular/core';")
            .NewLine()
            .Append("import {CrudAppService} from '../CrudAppService';")
            .NewLine()
            .Append("import {HttpClient} from '@angular/common/http';")
            .NewLine()
            .Append("import {Delete" + request.Name + "Input} from './dtos/Delete" + request.Name + "Input';")
            .NewLine()
            .Append("import {Get" + request.Name + "Input} from './dtos/Get" + request.Name + "Input';")
            .NewLine()
            .Append("import {Update" + request.Name + "Input} from './dtos/Update" + request.Name + "Input';")
            .NewLine()
            .Append("import {Create" + request.Name + "Input} from './dtos/Create" + request.Name + "Input';")
            .NewLine()
            .Append("import {" + request.Name + "FullOutput} from './dtos/" + request.Name + "FullOutput';")
            .NewLine(2);

        stringBuilder
            .Append("@Injectable({")
            .NewLine().InsertTab()
            .Append("providedIn: 'root',")
            .NewLine()
            .Append("})")
            .NewLine();

        stringBuilder
            .Append(
                $"export class {request.Name}Service extends CrudAppService<{request.Name}FullOutput, Create{request.Name}Input, Update{request.Name}Input, Get{request.Name}Input, Delete{request.Name}Input> ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab().Append("constructor(")
            .NewLine().InsertTab(2)
            .Append("@Inject(HttpClient) http: HttpClient,")
            .NewLine().InsertTab(2)
            .Append("@Inject(API_BASE_URL) baseUrl?: string")
            .NewLine().InsertTab().Append(") {")
            .NewLine().InsertTab(2).Append($"super('{request.Name}', http, baseUrl);")
            .NewLine().InsertTab().Append("}")
            .NewLine().Append("}");


        result.Stringify = stringBuilder.ToString();

        return result;
    }

    public override async Task<StringifyResult> CreateEnum(EnumRequest request, ServerCallContext context)
    {
        var stringifyResult = new StringifyResult();
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"export enum {request.Name} " + "{");
        stringBuilder.Append(Environment.NewLine);
        foreach (var value in request.Values)
        {
            stringBuilder.InsertTab();
            stringBuilder.Append(value.Name + " = " + value.Value + ",");
            stringBuilder.Append(Environment.NewLine);
        }

        stringBuilder.Append("}");

        stringifyResult.Stringify = stringBuilder.ToString();
        return stringifyResult;
    }

    public override async Task<ComponentResult> CreateComponents(Entity request, ServerCallContext context)
    {
        var result = new ComponentResult
        {
            ListComponent = new ComponentResultEto()
            {
                ComponentTsStringify = ComponentHelper.GetComponentTsStringify(request).ToString(),
                ComponentHtmlStringify = ComponentHelper.GetComponentHtmlStringify(request).ToString(),
                ComponentCssStringify = ComponentHelper.GetComponentCssStringify(request).ToString(),
            }
        };

        return result;
    }

    #region HelperMethods

    private StringBuilder GenerateCreateInput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        foreach (var enumProperty in request.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.Append("import {" + enumProperty.Type + "} from '../../enums/" + enumProperty.Type + "';")
                .NewLine();
        }

        stringBuilder
            .Append($"export class Create{request.Name}Input ")
            .Append("{")
            .NewLine();

        foreach (var property in request.Properties)
        {
            if (property.IsRelationalProperty && property.RelationType == RelationType.OneToOne)
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.RelationalEntityName.ToCamelCase() + "Id: " +
                            PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                                (int) property.RelationalEntityPrimaryKeyType));

                if (property.Nullable)
                {
                    stringBuilder.Append(" | undefined;");
                }

                stringBuilder.NewLine();
            }

            if (property.IsRelationalProperty)
            {
                continue;
            }

            stringBuilder.InsertTab()
                .Append(property.Name.ToCamelCase() + ": ")
                .Append(property.Type.ToTypeScriptDataType(property.Nullable, property.IsRelationalProperty,
                    (int) property.RelationType, property.IsEnumerateProperty))
                .NewLine();
        }

        stringBuilder.Append("}");
        return stringBuilder;
    }

    private StringBuilder GenerateFullOutput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        //import {LinePartOutput} from '../../line/dtos/LinePartOutput';
        foreach (var relationalProperty in request.Properties.Where(x => x.IsRelationalProperty))
        {
            stringBuilder.Append("import {" + relationalProperty.RelationalEntityName +
                                 "PartOutput} from '../../" + relationalProperty.RelationalEntityName.ToCamelCase() +
                                 "/dtos/" + relationalProperty.RelationalEntityName + "PartOutput';").NewLine();
        }

        foreach (var enumProperty in request.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.Append("import {" + enumProperty.Type + "} from '../../enums/" + enumProperty.Type + "';")
                .NewLine();
        }

        stringBuilder.NewLine()
            .Append($"export class {request.Name}FullOutput ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab()
            .Append(
                $"id: {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) request.PrimaryKeyType)};")
            .NewLine();
        foreach (var property in request.Properties)
        {
            if (property.IsRelationalProperty && property.RelationType == RelationType.OneToOne)
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.RelationalEntityName.ToCamelCase() + "Id: " +
                            PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                                (int) property.RelationalEntityPrimaryKeyType));

                if (property.Nullable)
                {
                    stringBuilder.Append(" | undefined;");
                }

                stringBuilder.NewLine();
            }

            stringBuilder.InsertTab()
                .Append(property.Name.ToCamelCase() + ": ")
                .Append(property.Type.ToTypeScriptDataType(property.Nullable, property.IsRelationalProperty,
                    (int) property.RelationType, property.IsEnumerateProperty))
                .NewLine();
        }

        stringBuilder.Append("}");
        return stringBuilder;
    }

    private StringBuilder GeneratePartOutput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        foreach (var relationalProperty in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            stringBuilder.Append("import {" + relationalProperty.RelationalEntityName +
                                 "PartOutput} from '../../" + relationalProperty.RelationalEntityName.ToCamelCase() +
                                 "/dtos/" + relationalProperty.RelationalEntityName + "PartOutput';").NewLine();
        }

        foreach (var enumProperty in request.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.Append("import {" + enumProperty.Type + "} from '../../enums/" + enumProperty.Type + "';")
                .NewLine();
        }

        stringBuilder
            .Append($"export class {request.Name}PartOutput ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab()
            .Append(
                $"id: {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) request.PrimaryKeyType)};")
            .NewLine();
        foreach (var property in request.Properties)
        {
            if (property.IsRelationalProperty && property.RelationType == RelationType.OneToOne)
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.RelationalEntityName.ToCamelCase() + "Id: " +
                            PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                                (int) property.RelationalEntityPrimaryKeyType));

                if (property.Nullable)
                {
                    stringBuilder.Append(" | undefined;");
                }

                stringBuilder.NewLine();
            }

            if (property.IsRelationalProperty && property.RelationType != RelationType.OneToOne)
            {
                continue;
            }

            stringBuilder.InsertTab()
                .Append(property.Name.ToCamelCase() + ": ")
                .Append(property.Type.ToTypeScriptDataType(property.Nullable, property.IsRelationalProperty,
                    (int) property.RelationType, property.IsEnumerateProperty))
                .NewLine();
        }

        stringBuilder.Append("}");
        return stringBuilder;
    }

    private StringBuilder GenerateUpdateInput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        foreach (var enumProperty in request.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.Append("import {" + enumProperty.Type + "} from '../../enums/" + enumProperty.Type + "';")
                .NewLine();
        }

        stringBuilder
            .Append($"export class Update{request.Name}Input ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab()
            .Append(
                $"id: {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) request.PrimaryKeyType)};")
            .NewLine();
        foreach (var property in request.Properties)
        {
            if (property.IsRelationalProperty && property.RelationType == RelationType.OneToOne)
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.RelationalEntityName.ToCamelCase() + "Id: " +
                            PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                                (int) property.RelationalEntityPrimaryKeyType));

                if (property.Nullable)
                {
                    stringBuilder.Append(" | undefined;");
                }

                stringBuilder.NewLine();
            }

            if (property.IsRelationalProperty)
            {
                continue;
            }

            stringBuilder.InsertTab()
                .Append(property.Name.ToCamelCase() + ": ")
                .Append(property.Type.ToTypeScriptDataType(property.Nullable, property.IsRelationalProperty,
                    (int) property.RelationType, property.IsEnumerateProperty))
                .NewLine();
        }

        stringBuilder.Append("}");
        return stringBuilder;
    }

    private StringBuilder GenerateGetInput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder
            .Append($"export class Get{request.Name}Input ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab()
            .Append(
                $"id: {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) request.PrimaryKeyType)};")
            .NewLine();

        stringBuilder.Append("}");
        return stringBuilder;
    }

    private StringBuilder GenerateDeleteInput(Entity request)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder
            .Append($"export class Delete{request.Name}Input ")
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab()
            .Append(
                $"id: {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) request.PrimaryKeyType)};")
            .NewLine();

        stringBuilder.Append("}");
        return stringBuilder;
    }

    #endregion
}