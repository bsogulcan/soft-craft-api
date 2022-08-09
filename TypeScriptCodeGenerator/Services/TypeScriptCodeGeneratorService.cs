using System.Text;
using Extensions;
using Grpc.Core;
using Humanizer;
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
            .NewLine()
            .Append("import {ResponceTypeWrap} from '../ResponseType';")
            .NewLine()
            .Append("import {Observable} from 'rxjs';")
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
            .NewLine().InsertTab().Append("}").NewLine();


        var relationalEntities = request.Properties.Where(x =>
             x.IsRelationalProperty && x.RelationType == RelationType.OneToOne);

        foreach (var relationalProperty in relationalEntities)
        {
            // getDepartmentsByFactoryId(id: number): Observable<ResponceTypeWrap<Array<DepartmentFullOutput>>> {
            //     return this.http.get<ResponceTypeWrap<Array<DepartmentFullOutput>>>(this.baseUrl + '/api/services/app/' + this.endPoint + '/GetDepartmentsByFactoryId?factoryId=' + id);
            // }

            stringBuilder.NewLine().InsertTab()
                .Append(
                    $"get{request.Name.Pluralize()}By{relationalProperty.Name}Id(id: {relationalProperty.Type}): Observable<ResponceTypeWrap<Array<{request.Name}FullOutput>>> ")
                .Append("{")
                .NewLine().InsertTab(2)
                .Append(
                    $"return this.http.get<ResponceTypeWrap<Array<{request.Name}FullOutput>>>(this.baseUrl + '/api/services/app/' + this.endPoint + '/Get{request.Name.Pluralize()}By{relationalProperty.Name}Id?{relationalProperty.Name.ToCamelCase()}Id=' + id);")
                .NewLine().InsertTab().Append("}")
                .NewLine();
        }

        if (relationalEntities.Count() > 1)
        {
            stringBuilder.NewLine().InsertTab()
            .Append($"getAll{request.Name.Pluralize()}Filtered(");

            bool isFirst = true;
            foreach (var relationalProperty in relationalEntities)
            {
                if (isFirst)
                {
                    stringBuilder
                        .Append($"{relationalProperty.Name.ToCamelCase()}Id : {relationalProperty.Type}");
                    isFirst = false;
                }
                else
                {
                    stringBuilder
                        .Append($", {relationalProperty.Name.ToCamelCase()}Id : {relationalProperty.Type}");
                }
            }
            stringBuilder
                .Append(") {")
                .NewLine()
                .InsertTab(2)
                .Append($"return this.http.get<ResponceTypeWrap<Array<{request.Name}FullOutput>>>(this.baseUrl + '/api/services/app/' + this.endPoint + '/GetAll{request.Name.Pluralize()}Filtered?");
            isFirst = true;
            foreach (var relationalProperty in relationalEntities)
            {
                if (isFirst)
                {
                    stringBuilder
                        .Append($"{relationalProperty.Name.ToCamelCase()}Id=' + {relationalProperty.Name.ToCamelCase()}Id + '");
                    isFirst = false;
                }
                else
                {
                    stringBuilder
                        .Append($"&{relationalProperty.Name.ToCamelCase()}Id=' + {relationalProperty.Name.ToCamelCase()}Id + '");
                }
            }
            stringBuilder
                .Append("');")
                .NewLine()
                .InsertTab()
                .Append("}")
                .NewLine(); ;
        }

        stringBuilder.NewLine().Append("}");

        result.Stringify = stringBuilder.ToString();

        return result;
    }

    public override async Task<StringifyResult> CreateEnum(EnumRequest request, ServerCallContext context)
    {
        var stringifyResult = new StringifyResult();
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("import {BaseEnum} from './BaseEnum';").NewLine();

        stringBuilder.Append($"export enum {request.Name} " + "{");
        stringBuilder.NewLine();
        foreach (var value in request.Values)
        {
            stringBuilder.InsertTab();
            stringBuilder.Append(value.Name + " = " + value.Value + ",");
            stringBuilder.NewLine();
        }

        stringBuilder.Append("}").NewLine(2);

        stringBuilder.Append($"export const {request.Name}List : Array<BaseEnum> = [").NewLine();
        foreach (var value in request.Values)
        {
            stringBuilder.InsertTab();
            stringBuilder.Append($"{{ id: {value.Value }, displayName: \"{value.Name}\" }},");
            stringBuilder.NewLine();
        }
        stringBuilder.Append("];");

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
            },
            CreateComponent = new ComponentResultEto()
            {
                ComponentTsStringify = CreateComponentHelper.GetCreateComponentTsStringify(request).ToString(),
                ComponentHtmlStringify = CreateComponentHelper.GetCreateComponentHtmlStringify(request).ToString(),
                ComponentCssStringify = CreateComponentHelper.GetCreateComponentCssStringify().ToString(),
            },
            EditComponent = new ComponentResultEto()
            {
                ComponentTsStringify = EditComponentHelper.GetEditComponentTsStringify(request).ToString(),
                ComponentHtmlStringify = EditComponentHelper.GetEditComponentHtmlStringify(request).ToString(),
                ComponentCssStringify = EditComponentHelper.GetEditComponentCssStringify().ToString(),
            },
        };

        return result;
    }

    public override async Task<StringifyResult> CreateNavigationItems(CreateNavigationItemRequest request,
        ServerCallContext context)
    {
        var result = new StringifyResult();
        // new MenuItem(
        //     this.l('Users'),
        //     '/app/users',
        //     'fas fa-users',
        //     'Pages.Users'
        // ),

        var stringBuilder = new StringBuilder();

        foreach (var navigation in request.Navigations)
        {
            stringBuilder.Append("new MenuItem(")
                .NewLine().InsertTab().Append($"this.l('{navigation.Caption}'),");

            if (navigation.HasEntityName)
            {
                stringBuilder.NewLine().InsertTab()
                    .Append($"'/app/{navigation.EntityName.ToCamelCase().Pluralize()}',");
            }
            else
            {
                stringBuilder.NewLine().InsertTab().Append("'',");
            }

            stringBuilder
                .NewLine().InsertTab().Append($"'{navigation.Icon}',");

            stringBuilder.NewLine().InsertTab().Append($"[{string.Join(", ", GetChildPermissionNames(navigation))}],");

            stringBuilder.NewLine().InsertTab().Append("[");

            foreach (var createNavigationItemRequest in navigation.Navigations)
            {
                stringBuilder.Append(GetMenuItem(createNavigationItemRequest));
            }

            stringBuilder.NewLine().InsertTab().Append("],");
            stringBuilder.NewLine().Append("),").NewLine();
        }

        result.Stringify = stringBuilder.ToString();

        return result;
    }

    #region HelperMethods

    private string GetMenuItem(NavigationItemRequest input)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.NewLine().Append("new MenuItem(")
            .NewLine().InsertTab().Append($"this.l('{input.Caption}'),");

        if (input.HasEntityName)
        {
            stringBuilder.NewLine().InsertTab()
                .Append($"'/app/{input.EntityName.ToCamelCase().Pluralize()}',");
        }
        else
        {
            stringBuilder.NewLine().InsertTab().Append("'',");
        }

        stringBuilder.NewLine().InsertTab().Append($"'{input.Icon}',");

        stringBuilder.NewLine().InsertTab().Append($"[{string.Join(", ", GetChildPermissionNames(input))}],");

        stringBuilder.NewLine().InsertTab().Append("[");

        foreach (var createNavigationItemRequest in input.Navigations)
        {
            stringBuilder.Append(GetMenuItem(createNavigationItemRequest));
        }

        stringBuilder.NewLine().InsertTab().Append("],")
            .NewLine().Append("),");


        return stringBuilder.ToString();
    }

    private List<String> GetChildPermissionNames(NavigationItemRequest input)
    {
        List<String> result = new List<String>();
        if (input.Navigations.Count > 0)
        {
            foreach (var curentNavigation in input.Navigations)
            {
                result.AddRange(GetChildPermissionNames(curentNavigation));
            }
        }
        else
        {
            if (input.HasEntityName)
            {
                result.Add($"'{input.EntityName}.Navigation'");
            }
        }
        return result;
    }

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
            if (property.IsRelationalProperty && (property.RelationType == RelationType.OneToOne || property.RelationType == RelationType.OneToZero))
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.Name.ToCamelCase() + "Id: " +
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
            if (relationalProperty.RelationalEntityName == "User")
                stringBuilder.Append("import { UserDto } from '@shared/service-proxies/service-proxies';").NewLine();
            else if (relationalProperty.RelationalEntityName == "Role")
                stringBuilder.Append("import { RoleDto } from '@shared/service-proxies/service-proxies';").NewLine();
            else
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
            if (property.IsRelationalProperty && (property.RelationType == RelationType.OneToOne || property.RelationType == RelationType.OneToZero))
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.Name.ToCamelCase() + "Id: " +
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
            if (property.IsRelationalProperty && (property.RelationType == RelationType.OneToOne || property.RelationType == RelationType.OneToZero))
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.Name.ToCamelCase() + "Id: " +
                            PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                                (int) property.RelationalEntityPrimaryKeyType));

                if (property.Nullable)
                {
                    stringBuilder.Append(" | undefined;");
                }

                stringBuilder.NewLine();
            }

            if (property.IsRelationalProperty && property.RelationType != RelationType.OneToOne && property.RelationType != RelationType.OneToZero)
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
            if (property.IsRelationalProperty && (property.RelationType == RelationType.OneToOne || property.RelationType == RelationType.OneToZero))
            {
                // standartId: number;
                stringBuilder.InsertTab()
                    .Append(property.Name.ToCamelCase() + "Id: " +
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