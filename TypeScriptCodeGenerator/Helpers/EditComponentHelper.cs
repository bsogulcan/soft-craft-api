﻿using System.Text;
using Extensions;
using Humanizer;
using TypeScriptCodeGenerator.Modals;

namespace TypeScriptCodeGenerator.Helpers;

public static class EditComponentHelper
{
    public static StringBuilder GetEditComponentTsStringify(Entity entity)
    {
        foreach (var property in entity.Properties)
        {
            property.Used = false;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append("import {Component, OnInit} from '@angular/core';")
            .NewLine().Append("import {DynamicDialogRef, DynamicDialogConfig} from 'primeng-lts/dynamicdialog';")
            .NewLine().Append(
                "import {" + entity.Name + "Service} from '../../../../shared/services/" +
                entity.Name + "/" + entity.Name.ToCamelCase() + ".service';")
            .NewLine().Append(
                "import {Update" + entity.Name + "Input} from '../../../../shared/services/" + entity.Name +
                "/dtos/Update" + entity.Name + "Input';")
            .NewLine().Append(
                "import {" + entity.Name + "FullOutput} from '../../../../shared/services/" + entity.Name +
                "/dtos/" + entity.Name + "FullOutput';")
            .NewLine().Append("import {FormControl, FormGroup, Validators} from '@angular/forms';");

        foreach (var item in entity.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.NewLine()
                .Append(
                    $"import {{{item.Type}, {item.Type}List}} from '../../../../shared/services/enums/{item.Type}';");
        }

        var relatedEntities = EntityHelper.GetRelatedEntities(entity);

        foreach (var relatedEntity in relatedEntities)
        {
            relatedEntity.Childs = EntityHelper.GetChildEntity(relatedEntity.Entity, relatedEntities);
            relatedEntity.Childs.Reverse();
        }

        relatedEntities.Reverse();

        GetRecursiveServicesAndDtosImports(stringBuilder, relatedEntities);

        stringBuilder.NewLine(2);

        stringBuilder.Append("@Component({")
            .NewLine().InsertTab().Append($"selector: 'app-edit-{entity.Name.ToCamelCase()}',")
            .NewLine().InsertTab().Append($"templateUrl: './edit-{entity.Name.ToCamelCase()}.component.html',")
            .NewLine().InsertTab().Append($"styleUrls: ['./edit-{entity.Name.ToCamelCase()}.component.css']")
            .NewLine().Append("})")
            .NewLine();

        stringBuilder.Append($"export class Edit{entity.Name}Component implements OnInit ").Append("{")
            .NewLine().InsertTab().Append($"updateInput = new Update{entity.Name}Input();")
            .NewLine().InsertTab().Append($"currentData : {entity.Name}FullOutput;")
            .NewLine().InsertTab().Append($"isSaving: boolean;")
            .NewLine().InsertTab().Append("formGroup: FormGroup;");

        foreach (var item in entity.Properties.Where(x => x.IsEnumerateProperty))
        {
            stringBuilder.NewLine().InsertTab().Append($"{item.Type.ToCamelCase()}List = {item.Type}List;");
        }

        //Ozan
        GetRecursiveRelationalDtos(stringBuilder, relatedEntities, entity.Properties.ToList(),
            entity.ComboBoxWrapper.ToList());

        stringBuilder.NewLine(2);

        stringBuilder.InsertTab().Append("constructor(private ref: DynamicDialogRef,")
            .NewLine().InsertTab(4).Append($"public config: DynamicDialogConfig,")
            .NewLine().InsertTab(4).Append($"private {entity.Name.ToCamelCase()}Service: {entity.Name}Service,");

        //Ozan
        GetRecursiveRelationalInjections(stringBuilder, relatedEntities);

        stringBuilder.NewLine().InsertTab().Append(") {")
            .NewLine();

        //Ozan
        GetRecursiveRelationalGetAllCalls(stringBuilder, relatedEntities);
        stringBuilder.InsertTab(2).Append("this.currentData = config.data?.item?.result;").NewLine();

        GetRecursiveRelationalInitialValues(stringBuilder, relatedEntities, entity.Properties.ToList(), entity);

        stringBuilder.InsertTab().Append("}")
            .NewLine();

        stringBuilder.NewLine().InsertTab().Append("ngOnInit(): void {")
            .NewLine().InsertTab(2).Append("this.formGroup = new FormGroup({");

        foreach (var property in entity.Properties)
        {
            if (property.IsRelationalProperty && property.RelationType == RelationType.OneToMany)
            {
                continue;
            }

            stringBuilder.NewLine().InsertTab(3)
                .Append(
                    $"element{property.Name}: new FormControl('', [{(!property.Nullable ? "Validators.required" : "")}]),");
        }

        stringBuilder.NewLine().InsertTab(2).Append("});")
            .NewLine().InsertTab().Append("}")
            .NewLine();


        stringBuilder.NewLine().InsertTab().Append("close(response: boolean) {")
            .NewLine().InsertTab(2).Append("if (this.ref) {")
            .NewLine().InsertTab(3).Append("this.ref.close(response);")
            .NewLine().InsertTab(2).Append("}")
            .NewLine().InsertTab().Append("}")
            .NewLine();

        stringBuilder.NewLine().InsertTab().Append("save() {")
            .NewLine().InsertTab(2).Append("this.isSaving = true;")
            .NewLine().InsertTab(2).Append("this." + entity.Name.ToCamelCase() +
                                           "Service.update(this.updateInput).subscribe(response => {")
            .NewLine().InsertTab(4).Append("this.isSaving = false;")
            .NewLine().InsertTab(4).Append("if (response) {")
            .NewLine().InsertTab(5).Append("this.ref.close(response.result);")
            .NewLine().InsertTab(4).Append("}")
            .NewLine().InsertTab(3).Append("},")
            .NewLine().InsertTab(3).Append("(error) => {")
            .NewLine().InsertTab(4).Append("this.isSaving = false;")
            .NewLine().InsertTab(4).Append("this.ref.close(false);")
            .NewLine().InsertTab(3).Append("});")
            .NewLine().InsertTab().Append("}")
            .NewLine(2);
        GetRecursiveRelationalGetMethods(stringBuilder, relatedEntities, entity);
        // foreach (var currentParent in entity.ParentEntities)
        // {
        //     GetRecursiveChildDropdownReset(stringBuilder, currentParent, currentParent.Name, true);
        // }

        stringBuilder.Append("}");


        return stringBuilder;
    }

    public static StringBuilder GetEditComponentHtmlStringify(Entity entity)
    {
        foreach (var property in entity.Properties)
        {
            property.Used = false;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"flex align-content-between\">")
            .NewLine().InsertTab().Append("<div class=\"flex align-items-center\">")
            .NewLine().InsertTab(2).Append("<div class=\"p-fluid mt-1\">")
            .NewLine().InsertTab(3).Append("<form [formGroup]=\"formGroup\">");

        var relatedEntities = EntityHelper.GetRelatedEntities(entity, canBeDuplicate: true);

        foreach (var relatedEntity in relatedEntities)
        {
            relatedEntity.Childs = EntityHelper.GetChildEntity(relatedEntity.Entity, relatedEntities);
            relatedEntity.Childs.Reverse();
        }

        relatedEntities.Reverse();
        GetRecursiveRelationalDropdownHtmls(stringBuilder, relatedEntities, entity.Properties.ToList());

        foreach (var property in entity.Properties.Where(x => !x.IsRelationalProperty))
        {
            stringBuilder.NewLine().Append(GenerateElement(property, entity.Name));
        }

        stringBuilder.NewLine().InsertTab(3).Append("</form>")
            .NewLine().InsertTab(2).Append("</div>")
            .NewLine().InsertTab().Append("</div>")
            .NewLine().InsertTab().Append("<div class=\"flex justify-content-end flex-wrap footer\">")
            .NewLine().InsertTab(2)
            .Append(
                "<button pButton pRipple label=\"{{ 'Cancel' | localize }}\" icon=\"pi pi-times\" class=\"p-button-text\" (click)=\"close(false)\" [disabled]=\"isSaving\"></button>")
            .NewLine().InsertTab(2)
            .Append(
                "<button pButton pRipple label=\"{{ 'Save' | localize }}\" icon=\"pi pi-check\" class=\"p-button-text\" (click)=\"save()\" [disabled]=\"isSaving || this.formGroup.invalid\"></button>")
            .NewLine().InsertTab().Append("</div>")
            .NewLine().Append("</div>");

        return stringBuilder;
    }

    private static string GenerateElement(Property property, string entityName)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.InsertTab(4).Append("<div class=\"p-field p-grid\">");
        stringBuilder.NewLine().InsertTab(5)
            .Append(
                "<label for=\"element" + property.Name +
                "\" class=\"p-col-12 p-mb-2 p-md-2 p-mb-md-0\">{{ \"" + property.Name +
                "\" | localize}}</label>")
            .NewLine().InsertTab(5).Append("<div class=\"p-col-12 p-md-10\">");
        switch (property.Type)
        {
            case "string":
            {
                stringBuilder.NewLine().InsertTab(6)
                    .Append(
                        "<input pInputText inputId=\"element" + property.Name +
                        "\" type=\"text\" formControlName=\"element" + property.Name + "\" [(ngModel)]=\"updateInput." +
                        property.Name.ToCamelCase() + "\" />");
            }
                break;
            case "int" or "long" or "float" or "double" or "decimal":
            {
                stringBuilder.NewLine().InsertTab(6)
                    .Append("<p-inputNumber inputId=\"element" + property.Name + "\" [(ngModel)]=\"updateInput." +
                            property.Name.ToCamelCase() + "\" formControlName=\"element" + property.Name +
                            "\" [showButtons]=\"true\" ></p-inputNumber>");
            }
                break;
            case "bool":
            {
                stringBuilder.NewLine().InsertTab(6)
                    .Append("<p-inputSwitch binary=\"true\" inputId=\"element" + property.Name +
                            "\"  [(ngModel)]=\"updateInput." + property.Name.ToCamelCase() +
                            "\" formControlName=\"element" + property.Name + "\"></p-inputSwitch>");
            }
                break;
            case "DateTime":
            {
                stringBuilder.NewLine().InsertTab(6)
                    .Append(
                        "<p-calendar appendTo=\"body\" [(ngModel)]=\"updateInput." +
                        property.Name.ToCamelCase() + "\" [showTime]=\"true\" inputId=\"element" +
                        property.Name +
                        "\" formControlName=\"element" + property.Name + "\"></p-calendar>");
            }
                break;
            default:
            {
                if (property.IsEnumerateProperty)
                {
                    stringBuilder.NewLine().InsertTab(6).Append(
                        $"<p-dropdown appendTo=\"body\" [options]=\"{property.Type.ToCamelCase()}List\"  [(ngModel)]=\"updateInput.{property.Name.ToCamelCase()}\" placeholder=\"{{{{ 'Select{property.Type}' | localize}}}}\"  formControlName=\"element{property.Name}\" optionLabel=\"displayName\" optionValue=\"id\" inputId=\"element{property.Name}\" [showClear]=\"true\"></p-dropdown>");
                }
            }
                break;
        }

        stringBuilder.NewLine().InsertTab(5).Append("</div>")
            .NewLine().InsertTab(4).Append("</div>");

        return stringBuilder.ToString();
    }

    public static StringBuilder GetEditComponentCssStringify()
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }

    #region Recursive Helpers

    public static void GetRecursiveServicesAndDtosImports(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities)
    {
        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.Name == "User")
            {
                stringBuilder.NewLine()
                    .Append("import {UserServiceProxy, UserDto} from '@shared/service-proxies/service-proxies';");
            }
            else if (relatedEntity.Entity.Name == "Role")
            {
                stringBuilder.NewLine()
                    .Append("import {RoleServiceProxy, RoleDto} from '@shared/service-proxies/service-proxies';");
            }
            else
            {
                stringBuilder.NewLine().Append(
                        "import {" + relatedEntity.Entity.Name + "Service} from '../../../../shared/services/" +
                        relatedEntity.Entity.Name + "/" + relatedEntity.Entity.Name.ToCamelCase() + ".service';")
                    .NewLine().Append(
                        "import {" + relatedEntity.Entity.Name + "FullOutput} from '../../../../shared/services/" +
                        relatedEntity.Entity.Name +
                        "/dtos/" + relatedEntity.Entity.Name + "FullOutput';");
            }
        }
    }

    public static void GetRecursiveRelationalDtos(StringBuilder stringBuilder, List<EntityWrapper> relatedEntities,
        List<Property> properties, List<ComboBoxWrapper> comboBoxWrappers)
    {
        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.Name == "User")
            {
                stringBuilder
                    .NewLine()
                    .InsertTab()
                    .Append(
                        $"{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} : Array<UserDto> = new Array<UserDto>();");
                stringBuilder.NewLine().InsertTab()
                    .Append(
                        $"selected{relatedEntity.Entity.Name}Id : {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) relatedEntity.Entity.PrimaryKeyType)};");

                foreach (var item in properties.Where(x => x.RelationalEntityName == relatedEntity.Entity.Name))
                {
                    stringBuilder.NewLine().InsertTab()
                        .Append(
                            $"selected{item.Name}Id : {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) relatedEntity.Entity.PrimaryKeyType)};");
                }
            }
            else if (relatedEntity.Entity.Name == "Role")
            {
                stringBuilder
                    .NewLine()
                    .InsertTab()
                    .Append(
                        $"{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} : Array<RoleDto> = new Array<RoleDto>();");
                stringBuilder.NewLine().InsertTab()
                    .Append(
                        $"selected{relatedEntity.Entity.Name.ToCamelCase()}Id : {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) relatedEntity.Entity.PrimaryKeyType)};");

                foreach (var item in properties.Where(x => x.RelationalEntityName == relatedEntity.Entity.Name))
                {
                    stringBuilder.NewLine().InsertTab()
                        .Append(
                            $"selected{item.Name}Id : {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) relatedEntity.Entity.PrimaryKeyType)};");
                }
            }
            else
            {
                stringBuilder
                    .NewLine()
                    .InsertTab()
                    .Append(
                        $"{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} : Array<{relatedEntity.Entity.Name}FullOutput> = new Array<{relatedEntity.Entity.Name}FullOutput>();");

                foreach (var item in comboBoxWrappers.Where(x =>
                             x.EntityName == relatedEntity.Entity.Name && !x.IsInputProperty))
                {
                    stringBuilder.NewLine()
                        .InsertTab()
                        .Append(
                            $"selected{item.PropertyName}Id : {PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType((int) relatedEntity.Entity.PrimaryKeyType)};");
                }
            }
        }
    }

    public static void GetRecursiveRelationalInjections(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities)
    {
        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.Name == "User")
            {
                stringBuilder
                    .NewLine()
                    .InsertTab(4)
                    .Append(
                        $"private {relatedEntity.Entity.Name.ToCamelCase()}Service: UserServiceProxy,");
            }
            else if (relatedEntity.Entity.Name == "Role")
            {
                stringBuilder
                    .NewLine()
                    .InsertTab(4)
                    .Append(
                        $"private {relatedEntity.Entity.Name.ToCamelCase()}Service: RoleServiceProxy,");
            }
            else
            {
                stringBuilder
                    .NewLine()
                    .InsertTab(4)
                    .Append(
                        $"private {relatedEntity.Entity.Name.ToCamelCase()}Service: {relatedEntity.Entity.Name}Service,");
            }
        }
    }

    public static void GetRecursiveRelationalGetAllCalls(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities)
    {
        foreach (var relatedEntity in relatedEntities.Where(x => x.Entity.ParentEntities.Count == 0))
        {
            stringBuilder
                .InsertTab(2)
                .Append("this.getAll" + relatedEntity.Entity.Name.Pluralize() + "();")
                .NewLine();
        }
    }

    public static void GetRecursiveRelationalGetMethods(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities, Entity entity)
    {
        var properties = entity.Properties.ToList();

        foreach (var property in properties)
        {
            property.Used = false;
        }

        foreach (var relatedEntity in relatedEntities)
        {
            if (relatedEntity.Entity.ParentEntities.Count == 0)
            {
                if (relatedEntity.Entity.Name == "User")
                {
                    stringBuilder
                        .NewLine()
                        .InsertTab(1)
                        .Append("getAll" + relatedEntity.Entity.Name.Pluralize() + "() {")
                        .NewLine()
                        .InsertTab(2)
                        .Append(
                            $"this.{relatedEntity.Entity.Name.ToCamelCase()}Service.getAll(undefined, undefined, undefined, undefined).subscribe(")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(response) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("if (response) {")
                        .NewLine()
                        .InsertTab(5)
                        .Append($"this.{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} = response.items;")
                        .NewLine().InsertTab(4)
                        .Append("} else {")
                        .NewLine()
                        .InsertTab(5)
                        .NewLine()
                        .InsertTab(4)
                        .Append("}")
                        .NewLine()
                        .InsertTab(3)
                        .Append("},")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(error) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("abp.message.error(error.error.error.message);")
                        .NewLine()
                        .InsertTab(3)
                        .Append("}")
                        .NewLine()
                        .InsertTab(2)
                        .Append(");")
                        .NewLine()
                        .InsertTab(1)
                        .Append("}")
                        .NewLine();
                }
                else if (relatedEntity.Entity.Name == "Role")
                {
                    stringBuilder
                        .NewLine()
                        .InsertTab(1)
                        .Append("getAll" + relatedEntity.Entity.Name.Pluralize() + "() {")
                        .NewLine()
                        .InsertTab(2)
                        .Append(
                            $"this.{relatedEntity.Entity.Name.ToCamelCase()}Service.getAll(undefined, undefined, undefined).subscribe(")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(response) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("if (response) {")
                        .NewLine()
                        .InsertTab(5)
                        .Append($"this.{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} = response.items;")
                        .NewLine().InsertTab(4)
                        .Append("} else {")
                        .NewLine()
                        .InsertTab(5)
                        .NewLine()
                        .InsertTab(4)
                        .Append("}")
                        .NewLine()
                        .InsertTab(3)
                        .Append("},")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(error) => {")
                        .NewLine()
                        .InsertTab(5)
                        .NewLine()
                        .Append("abp.message.error(error.error.error.message);")
                        .InsertTab(3)
                        .Append("}")
                        .NewLine()
                        .InsertTab(2)
                        .Append(");")
                        .NewLine()
                        .InsertTab(1)
                        .Append("}")
                        .NewLine();
                }
                else
                {
                    stringBuilder
                        .NewLine()
                        .InsertTab(1)
                        .Append("getAll" + relatedEntity.Entity.Name.Pluralize() + "() {")
                        .NewLine()
                        .InsertTab(2)
                        .Append($"this.{relatedEntity.Entity.Name.ToCamelCase()}Service.getList().subscribe(")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(response) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("if (response.success) {")
                        .NewLine()
                        .InsertTab(5)
                        .Append($"this.{relatedEntity.Entity.Name.ToCamelCase().Pluralize()} = response.result.items;")
                        .NewLine().InsertTab(4)
                        .Append("} else {")
                        .NewLine()
                        .InsertTab(5)
                        .Append("abp.message.error(response.error.message);")
                        .NewLine()
                        .InsertTab(4)
                        .Append("}")
                        .NewLine()
                        .InsertTab(3)
                        .Append("},")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(error) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("abp.message.error(error.error.error.message);")
                        .NewLine()
                        .InsertTab(3)
                        .Append("}")
                        .NewLine()
                        .InsertTab(2)
                        .Append(");")
                        .NewLine()
                        .InsertTab(1)
                        .Append("}")
                        .NewLine();
                }
            }

            if (relatedEntity.Childs.Count == 0)
            {
                continue;
            }

            stringBuilder.NewLine().InsertTab()
                .Append(
                    $"on{relatedEntity.Entity.Name}Changed({relatedEntity.Entity.Name.ToCamelCase()}Id?: number, fromInit = false) ")
                .Append("{")
                .NewLine()
                .InsertTab(2);


            stringBuilder.Append($"if (!fromInit) ")
                .Append("{")
                .NewLine();

            foreach (var child in relatedEntity.Childs)
            {
                var mainProperty =
                    entity.ComboBoxWrapper.First(x =>
                        x.EntityName == child); //EntityHelper.GetProperty(ref properties, child);

                if (mainProperty.IsInputProperty)
                {
                    stringBuilder.InsertTab(3)
                        .Append($"this.updateInput.{mainProperty.PropertyName.ToCamelCase()}Id = undefined;");
                    stringBuilder.NewLine().InsertTab(3).Append($"this.{child.ToCamelCase().Pluralize()}.splice(0);");
                }
                else
                {
                    stringBuilder.NewLine().InsertTab(3).Append($"this.selected{child}Id = undefined;");
                    stringBuilder.NewLine().InsertTab(3).Append($"this.{child.ToCamelCase().Pluralize()}.splice(0);");
                }
            }

            stringBuilder.NewLine().InsertTab(2).Append("}");

            foreach (var child in relatedEntity.Childs)
            {
                if (relatedEntities.First(x => x.Entity.Name == child).Entity.ParentEntities
                    .Any(x => x.Name != relatedEntity.Entity.Name))
                {
                    continue;
                }

                stringBuilder.NewLine(2).InsertTab(2).Append($"if (!{relatedEntity.Entity.Name.ToCamelCase()}Id) ")
                    .Append("{")
                    .NewLine().InsertTab(3).Append("return;")
                    .NewLine().InsertTab(2).Append("}");

                var parentEntityProperties =
                    relatedEntities.First(x => x.Entity.Name == child).Entity.Properties.ToList();
                foreach (var parentEntityProperty in parentEntityProperties)
                {
                    parentEntityProperty.Used = false;
                }

                var property =
                    EntityHelper.GetProperty(
                        ref parentEntityProperties, relatedEntity.Entity.Name);

                stringBuilder.NewLine(2)
                    .InsertTab(2).Append("this." + child.ToCamelCase())
                    .Append(
                        $"Service.get{child.Pluralize()}By{property.Name}Id({relatedEntity.Entity.Name.ToCamelCase()}Id).subscribe(response => ")
                    .Append("{")
                    .NewLine()
                    .InsertTab(3)
                    .Append("if (response.success) {")
                    .NewLine()
                    .InsertTab(4)
                    .Append($"this.{child.ToCamelCase().Pluralize()} = response.result.items;")
                    .NewLine()
                    .InsertTab(3)
                    .Append("} else {")
                    .NewLine()
                    .InsertTab(4)
                    .Append("abp.message.error(response.error.message);")
                    .NewLine()
                    .InsertTab(3)
                    .Append("}")
                    .NewLine()
                    .InsertTab(2)
                    .Append("}, error => abp.message.error(error.error.error.message));")
                    .NewLine();
            }

            stringBuilder.InsertTab()
                .Append("}")
                .NewLine(2);
        }
    }

    public static void GetRecursiveRelationalDropdownHtmls(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities,
        List<Property> properties)
    {
        foreach (var property in properties)
        {
            property.Used = false;
        }

        foreach (var relatedEntity in relatedEntities)
        {
            var currentEntityProperty = EntityHelper.GetProperty(ref properties, relatedEntity.Entity.Name);

            stringBuilder.NewLine().InsertTab(4).Append("<div class=\"p-field p-grid\">")
                .NewLine().InsertTab(5)
                .Append(
                    $"<label for=\"element{(currentEntityProperty != null ? currentEntityProperty.Name : relatedEntity.Entity.Name)}\" class=\"p-col-12 p-mb-2 p-md-2 p-mb-md-0\">{{{{ '{(currentEntityProperty != null ? currentEntityProperty.Name : relatedEntity.Entity.Name)}' | localize }}}}</label>")
                .NewLine().InsertTab(5).Append("<div class=\"p-col-12 p-md-10\">");

            if (relatedEntity.MainEntity)
            {
                stringBuilder.NewLine().InsertTab(6).Append(
                    $"<p-dropdown appendTo=\"body\" [options]=\"{relatedEntity.Entity.Name.Pluralize().ToCamelCase()}\"  [(ngModel)]=\"updateInput.{currentEntityProperty.Name.ToCamelCase()}Id\" placeholder=\"{{{{ 'Select{relatedEntity.Entity.Name}' | localize}}}}\" [filter]=\"true\" filterBy=\"{string.Join(",", relatedEntity.Entity.Properties.Where(x => x.FilterOnList).Select(x => x.Name.ToCamelCase()))}\" formControlName=\"element{(currentEntityProperty != null ? currentEntityProperty.Name : relatedEntity.Entity.Name)}\" optionLabel=\"{relatedEntity.Entity.Properties.FirstOrDefault(x => x.DisplayOnList)?.Name.ToCamelCase()}\" optionValue=\"id\" inputId=\"element{(currentEntityProperty != null ? currentEntityProperty.Name : relatedEntity.Entity.Name)}\" [showClear]=\"true\">");
            }
            else
            {
                stringBuilder.NewLine().InsertTab(6).Append(
                    $"<p-dropdown appendTo=\"body\" [options]=\"{relatedEntity.Entity.Name.Pluralize().ToCamelCase()}\"  [(ngModel)]=\"selected{relatedEntity.Entity.Name}Id\" [ngModelOptions]=\"{{ standalone: true }}\" placeholder =\"{{{{ 'Select{relatedEntity.Entity.Name}' | localize}}}}\" [filter]=\"true\" filterBy=\"{string.Join(",", relatedEntity.Entity.Properties.Where(x => x.FilterOnList).Select(x => x.Name.ToCamelCase()))}\" optionLabel=\"{relatedEntity.Entity.Properties.FirstOrDefault(x => x.DisplayOnList)?.Name.ToCamelCase()}\" inputId=\"element{relatedEntity.Entity.Name}\" optionValue=\"id\" [showClear]=\"true\" (onChange)=\"on{relatedEntity.Entity.Name}Changed($event.value)\">");
            }

            stringBuilder.NewLine().InsertTab(7).Append("<ng-template let-item pTemplate=\"item\">");
            stringBuilder.NewLine().InsertTab(8).Append("<div>");
            stringBuilder.NewLine().InsertTab(9)
                .Append(
                    $"<div>{string.Join(' ', relatedEntity.Entity.Properties.Where(x => x.DisplayOnList).Select(x => string.Format("{{{{item.{0}}}}}", x.Name.ToCamelCase())))}</div>");
            stringBuilder.NewLine().InsertTab(8).Append("</div>");
            stringBuilder.NewLine().InsertTab(7).Append("</ng-template>")
                .NewLine().InsertTab(6).Append("</p-dropdown>")
                .NewLine().InsertTab(5).Append("</div>")
                .NewLine().InsertTab(4).Append("</div>");
        }
    }

    public static void GetRecursiveChildDropdownReset(StringBuilder stringBuilder, Entity entity, String EntityName,
        bool isFirst = false, Dictionary<string, HashSet<string>>? Existence = null)
    {
        //if (Existence == null)
        //    Existence = new Dictionary<string, HashSet<string>>();

        //foreach (var currentParent in entity.ParentEntities)
        //{
        //    GetRecursiveChildDropdownReset(stringBuilder, currentParent, EntityName, true, Existence);
        //    foreach (var currentGrandParent in currentParent.ParentEntities)
        //    {
        //        GetRecursiveChildDropdownReset(stringBuilder, currentGrandParent, currentParent.Name, false, Existence);
        //    }
        //}
        //var searchText = $"on{entity.Name}Changed({entity.Name.ToCamelCase()}Id: number) ";
        //searchText = searchText + "{";
        //var currentText = stringBuilder.ToString().IndexOf(searchText);
        //if (currentText != -1)
        //{
        //    if (Existence.ContainsKey(entity.Name))
        //    {
        //        if (!Existence[entity.Name].Contains(EntityName))
        //        {
        //            StringBuilder temp = new StringBuilder();
        //            if (isFirst)
        //                temp.NewLine().InsertTab(2).Append($"this.updateInput.{EntityName.ToCamelCase()}Id = undefined;");
        //            else
        //                temp.NewLine().InsertTab(2).Append($"this.selected{EntityName}Id = undefined;");
        //            temp.NewLine().InsertTab(2).Append($"this.{EntityName.ToCamelCase().Pluralize()}.splice(0);");
        //            stringBuilder.Insert(currentText + searchText.Length, temp.ToString());
        //            Existence[entity.Name].Add(EntityName);
        //        }
        //    }
        //    else
        //    {
        //        Existence.Add(entity.Name, new HashSet<string>());
        //        StringBuilder temp = new StringBuilder();
        //        if (isFirst)
        //            temp.NewLine().InsertTab(2).Append($"this.updateInput.{EntityName.ToCamelCase()}Id = undefined;");
        //        else
        //            temp.NewLine().InsertTab(2).Append($"this.selected{EntityName}Id = undefined;");
        //        temp.NewLine().InsertTab(2).Append($"this.{EntityName.ToCamelCase().Pluralize()}.splice(0);");
        //        stringBuilder.Insert(currentText + searchText.Length, temp.ToString());
        //        Existence[entity.Name].Add(EntityName);
        //    }
        //}
    }

    public static void GetRecursiveRelationalInitialValues(StringBuilder stringBuilder,
        List<EntityWrapper> relatedEntities, List<Property> properties, Entity entity)
    {
        foreach (var property in properties)
        {
            property.Used = false;
        }

        foreach (var comboBoxWrapper in entity.ComboBoxWrapper.Where(x => !x.IsInputProperty))
        {
            stringBuilder.InsertTab(2)
                .Append(
                    $"this.selected{comboBoxWrapper.EntityName}Id = this.currentData.{comboBoxWrapper.AccessString.Replace("createInput.", "") + ".id;"}")
                .NewLine();
            stringBuilder.InsertTab(2)
                .Append(
                    $"this.on{comboBoxWrapper.EntityName}Changed(this.selected{comboBoxWrapper.EntityName}Id, true);")
                .NewLine();
        }

        stringBuilder.InsertTab(2).Append("this.updateInput = JSON.parse(JSON.stringify(this.currentData));").NewLine();

        foreach (var dateProperty in properties.Where(x => x.Type == "DateTime"))
        {
            stringBuilder.InsertTab(2)
                .Append(
                    $"this.updateInput.{dateProperty.Name.ToCamelCase()} = new Date(this.updateInput.{dateProperty.Name.ToCamelCase()});")
                .NewLine();
        }
        // foreach (var relatedEntity in relatedEntities)
        // {
        //     var mainEntityProperty = EntityHelper.GetProperty(ref properties, relatedEntity.Entity.Name);
        //
        //     // if (relatedEntity.MainEntity)
        //     // {
        //     //     stringBuilder.InsertTab(2)
        //     //         .Append(
        //     //             $"this.updateInput.{mainEntityProperty.Name.ToCamelCase()}Id = this.currentData.{mainEntityProperty.Name.ToCamelCase()}Id;")
        //     //         .NewLine();
        //     // }
        //     // else
        //     // {
        //     var dataField = string.Empty;
        //     //TODO: needs to be refactoring
        //     foreach (var relatedEntityChild in relatedEntity.Childs)
        //     {
        //         var relatedEntityInfo = relatedEntities.First(x => x.Entity.Name == relatedEntityChild).Entity;
        //         mainEntityProperty = EntityHelper.GetProperty(ref properties, relatedEntityInfo.Name, false);
        //         dataField += mainEntityProperty.Name.ToCamelCase() + '.';
        //
        //
        //         var list = relatedEntityInfo.Properties.ToList();
        //         mainEntityProperty =
        //             EntityHelper.GetProperty(ref list, relatedEntity.Entity.Name, false);
        //         dataField += mainEntityProperty.Name.ToCamelCase();
        //     }
        //
        //     stringBuilder.InsertTab(2)
        //         .Append(
        //             $"this.selected{relatedEntity.Entity.Name}Id = this.currentData.{dataField + ".id;"}")
        //         .NewLine();
        //     stringBuilder.InsertTab(2)
        //         .Append($"this.on{relatedEntity.Entity.Name}Changed(this.selected{relatedEntity.Entity.Name}Id);")
        //         .NewLine();
        //     // }
        // }
    }

    #endregion
}