﻿using System.Text;
using System.Xml;
using Extensions;
using Humanizer;

namespace TypeScriptCodeGenerator.Helpers;

public static class CreateComponentHelper
{
    public static StringBuilder GetCreateComponentTsStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("import {Component, OnInit} from '@angular/core';")
            .NewLine().Append("import {DynamicDialogRef} from 'primeng-lts/dynamicdialog';")
            .NewLine().Append(
                "import {" + entity.Name + "Service} from '../../../../shared/services/" +
                entity.Name + "/" + entity.Name.ToCamelCase() + ".service';")
            .NewLine().Append(
                "import {Create" + entity.Name + "Input} from '../../../../shared/services/" + entity.Name +
                "/dtos/Create" + entity.Name + "Input';")
            .NewLine().Append("import {FormControl, FormGroup, Validators} from '@angular/forms';");

        GetRecursiveServicesAndDtosImports(stringBuilder, entity);

        stringBuilder.NewLine(2);

        stringBuilder.Append("@Component({")
            .NewLine().InsertTab().Append($"selector: 'app-create-{entity.Name.ToCamelCase()}',")
            .NewLine().InsertTab().Append($"templateUrl: './create-{entity.Name.ToCamelCase()}.component.html',")
            .NewLine().InsertTab().Append($"styleUrls: ['./create-{entity.Name.ToCamelCase()}.component.css']")
            .NewLine().Append("})")
            .NewLine();

        stringBuilder.Append($"export class Create{entity.Name}Component implements OnInit ").Append("{")
            .NewLine().InsertTab().Append($"createInput = new Create{entity.Name}Input();")
            .NewLine().InsertTab().Append($"isSaving: boolean;")
            .NewLine().InsertTab().Append("formGroup: FormGroup;");

        //Ozan
        GetRecursiveRelationalDtos(stringBuilder, entity);

        stringBuilder.NewLine(2);

        stringBuilder.InsertTab().Append("constructor(private ref: DynamicDialogRef,")
            .NewLine().InsertTab(4).Append($"private {entity.Name.ToCamelCase()}Service: {entity.Name}Service,");

        //Ozan
        GetRecursiveRelationalInjections(stringBuilder, entity);

        stringBuilder.NewLine().InsertTab().Append(") {")
            .NewLine();

        //Ozan
        GetRecursiveRelationalGetAllCalls(stringBuilder, entity);

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
                                           "Service.create(this.createInput).subscribe(response => {")
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
        GetRecursiveRelationalGetMethods(stringBuilder, entity);
        //Entity lastRelatedEntity = null;
        //foreach (var relatedEntity in entity.RelatedEntities)
        //{
        //    if (lastRelatedEntity != null)
        //    {
        //        stringBuilder.NewLine().InsertTab()
        //            .Append($"on{lastRelatedEntity.Name}Changed({lastRelatedEntity.Name.ToCamelCase()}Id: number) ")
        //            .Append("{")
        //            .NewLine().InsertTab(2).Append("this." + relatedEntity.Name.ToCamelCase() +
        //                                           $"Service.get{relatedEntity.Name.Pluralize()}By{lastRelatedEntity.Name}Id({lastRelatedEntity.Name.ToCamelCase()}Id).subscribe(response => ")
        //            .Append("{")
        //            .NewLine().InsertTab(3).Append("if (response.success) {")
        //            .NewLine().InsertTab(4)
        //            .Append($"this.{relatedEntity.Name.ToCamelCase().Pluralize()} = response.result.items;")
        //            .NewLine().InsertTab(3).Append("} else {")
        //            .NewLine().InsertTab(4).Append("abp.message.error(response.error.message);")
        //            .NewLine().InsertTab(3).Append("}")
        //            .NewLine().InsertTab(2).Append("}, error => abp.message.error(error.error.error.message));")
        //            .NewLine().InsertTab().Append("}")
        //            .NewLine(2);
        //    }

        //    lastRelatedEntity = relatedEntity;
        //}

        stringBuilder.Append("}");


        return stringBuilder;
    }

    public static StringBuilder GetCreateComponentHtmlStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"flex align-content-between\">")
            .NewLine().InsertTab().Append("<div class=\"flex align-items-center\">")
            .NewLine().InsertTab(2).Append("<div class=\"p-fluid mt-1\">")
            .NewLine().InsertTab(3).Append("<form [formGroup]=\"formGroup\">");


        foreach (var property in entity.Properties)
        {
            stringBuilder.NewLine().Append(GenerateElement(property, entity.Name));
        }

        stringBuilder.NewLine().InsertTab(3).Append("</form>")
            .NewLine().InsertTab(2).Append("</div>")
            .NewLine().InsertTab().Append("</div>")
            .NewLine().InsertTab().Append("<div class=\"flex\" align=\"end\">")
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

        switch (property.Type)
        {
            case "string":
            {
                stringBuilder.InsertTab(4).Append("<div class=\"p-field p-grid\">")
                    .NewLine().InsertTab(5)
                    .Append(
                        "<label for=\"element" + property.Name +
                        "\" class=\"p-col-12 p-mb-2 p-md-2 p-mb-md-0\">{{ \"" + property.Name +
                        "\" | localize}}</label>")
                    .NewLine().InsertTab(5).Append("<div class=\"p-col-12 p-md-10\">")
                    .NewLine().InsertTab(6)
                    .Append(
                        "<input pInputText inputId=\"element" + property.Name +
                        "\" type=\"text\" formControlName=\"element" + property.Name +
                        "\" [(ngModel)]=\"createInput." + property.Name.ToCamelCase() + "\" />")
                    .NewLine().InsertTab(5).Append("</div>")
                    .NewLine().InsertTab(4).Append("</div>");
            }
                break;
        }

        return stringBuilder.ToString();
    }

    public static StringBuilder GetCreateComponentCssStringify()
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }

    #region Recursive Helpers
    public static void GetRecursiveServicesAndDtosImports(StringBuilder stringBuilder, Entity entity, HashSet<Entity>? entities = null)
    {
        if (entities == null)
            entities = new HashSet<Entity>();

        foreach (var relatedEntity in entity.ParentEntities)
        {
            if (entities?.Contains(relatedEntity) == false)
            {
                stringBuilder.NewLine().Append(
                        "import {" + relatedEntity.Name + "Service} from '../../../../shared/services/" +
                        relatedEntity.Name + "/" + relatedEntity.Name.ToCamelCase() + ".service';")
                    .NewLine().Append(
                        "import {" + relatedEntity.Name + "FullOutput} from '../../../../shared/services/" +
                        relatedEntity.Name +
                        "/dtos/" + relatedEntity.Name + "FullOutput';");
                entities?.Add(relatedEntity);
                GetRecursiveServicesAndDtosImports(stringBuilder, relatedEntity, entities);
            }
        }
    }
    public static void GetRecursiveRelationalDtos(StringBuilder stringBuilder, Entity entity, HashSet<Entity>? entities = null)
    {
        if (entities == null)
            entities = new HashSet<Entity>();

        foreach (var relatedEntity in entity.ParentEntities)
        {
            if (entities?.Contains(relatedEntity) == false)
            {
                stringBuilder
                    .NewLine()
                    .InsertTab()
                    .Append(
                        $"{relatedEntity.Name.ToCamelCase().Pluralize()}: Array<{relatedEntity.Name}FullOutput> = new Array<{relatedEntity.Name}FullOutput>();");
                entities?.Add(relatedEntity);
                GetRecursiveRelationalDtos(stringBuilder, relatedEntity, entities);
            }
        }
    }
    public static void GetRecursiveRelationalInjections(StringBuilder stringBuilder, Entity entity, HashSet<Entity>? entities = null)
    {
        if (entities == null)
            entities = new HashSet<Entity>();

        foreach (var relatedEntity in entity.ParentEntities)
        {
            if (entities?.Contains(relatedEntity) == false)
            {
                stringBuilder
                    .NewLine()
                    .InsertTab(4)
                    .Append($"private {relatedEntity.Name.ToCamelCase()}Service: {relatedEntity.Name}Service,");
                entities?.Add(relatedEntity);
                GetRecursiveRelationalInjections(stringBuilder, relatedEntity, entities);
            }
        }
    }
    public static void GetRecursiveRelationalGetAllCalls(StringBuilder stringBuilder, Entity entity, HashSet<Entity>? entities = null)
    {
        if (entities == null)
            entities = new HashSet<Entity>();

        foreach (var relatedEntity in entity.ParentEntities)
        {
            if (entities?.Contains(relatedEntity) == false)
            {
                if (relatedEntity.ParentEntities.Count == 0)
                {
                    stringBuilder
                        .InsertTab(2)
                        .Append("this.getAll" + relatedEntity.Name.Pluralize() + "();")
                        .NewLine();
                }
                entities?.Add(relatedEntity);
                GetRecursiveRelationalGetAllCalls(stringBuilder, relatedEntity, entities);
            }
        }
    }
    public static void GetRecursiveRelationalGetMethods(StringBuilder stringBuilder, Entity entity, HashSet<Entity>? entities = null)
    {
        if (entities == null)
            entities = new HashSet<Entity>();

        foreach (var relatedEntity in entity.ParentEntities)
        {
            if (entities?.Contains(relatedEntity) == false)
            {
                if (relatedEntity.ParentEntities.Count == 0)
                {
                    stringBuilder
                        .NewLine()
                        .InsertTab(1)
                        .Append("getAll" + relatedEntity.Name.Pluralize() + "() {")
                        .NewLine()
                        .InsertTab(2)
                        .Append($"this.{relatedEntity.Name.ToCamelCase()}Service.getList().subscribe(")
                        .NewLine()
                        .InsertTab(3)
                        .Append("(response) => {")
                        .NewLine()
                        .InsertTab(4)
                        .Append("if (response.success) {")
                        .NewLine()
                        .InsertTab(5)
                        .Append($"this.{relatedEntity.Name.ToCamelCase().Pluralize()} = response.result.items;")
                        .NewLine().
                        InsertTab(4)
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
                        .InsertTab(5)
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
                else
                {
                    foreach (var currenParent in relatedEntity.ParentEntities)
                    {
                        stringBuilder.NewLine().InsertTab()
                            .Append($"on{currenParent.Name}Changed({currenParent.Name.ToCamelCase()}Id: number) ")
                            .Append("{")
                            .NewLine()
                            .InsertTab(2)
                            .Append("this." + relatedEntity.Name.ToCamelCase())
                            .Append($"Service.get{relatedEntity.Name.Pluralize()}By{currenParent.Name}Id({currenParent.Name.ToCamelCase()}Id).subscribe(response => ")
                            .Append("{")
                            .NewLine()
                            .InsertTab(3)
                            .Append("if (response.success) {")
                            .NewLine()
                            .InsertTab(4)
                            .Append($"this.{relatedEntity.Name.ToCamelCase().Pluralize()} = response.result.items;")
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
                            .NewLine()
                            .InsertTab()
                            .Append("}")
                            .NewLine(2);
                    }
                }
                entities?.Add(relatedEntity);
                GetRecursiveRelationalGetMethods(stringBuilder, relatedEntity, entities);
            }
        }
    }
    #endregion

}