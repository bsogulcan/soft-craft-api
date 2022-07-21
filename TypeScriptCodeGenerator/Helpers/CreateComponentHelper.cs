using System.Text;
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
                "import {" + entity.Name + "Service} from '../../../../../shared/services/" +
                entity.Name + "/" + entity.Name.ToCamelCase() + ".service';")
            .NewLine().Append(
                "import {Create" + entity.Name + "Input} from '../../../../shared/services/" + entity.Name +
                "/dtos/Create" + entity.Name + "Input';")
            .NewLine().Append("import {FormControl, FormGroup, Validators} from '@angular/forms';");

        foreach (var relationalProperty in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            stringBuilder.NewLine().Append(
                    "import {" + relationalProperty.Name + "Service} from '../../../../shared/services/" +
                    relationalProperty.Name + "/" + relationalProperty.Name.ToCamelCase() + ".service';")
                .NewLine().Append(
                    "import {" + relationalProperty.Name + "FullOutput} from '../../../../shared/services/" +
                    relationalProperty.Name +
                    "/dtos/" + relationalProperty.Name + "FullOutput';");
        }

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

        foreach (var relationalProperty in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            stringBuilder.NewLine().InsertTab()
                .Append(
                    $"{relationalProperty.Name.ToCamelCase().Pluralize()}: Array<{relationalProperty.Name}FullOutput>;");
        }

        stringBuilder.NewLine(2);

        stringBuilder.InsertTab().Append("constructor(private ref: DynamicDialogRef,")
            .NewLine().InsertTab(4).Append($"private {entity.Name.ToCamelCase()}Service: {entity.Name}Service,");

        foreach (var relationalProperty in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            stringBuilder.NewLine().InsertTab(4)
                .Append($"private {relationalProperty.Name.ToCamelCase()}Service: {relationalProperty.Name}Service,");
        }

        stringBuilder.NewLine().InsertTab().Append(") {")
            .NewLine();

        foreach (var relationalProperty in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            stringBuilder.NewLine().InsertTab(2).Append("this." + relationalProperty.Name.ToCamelCase() +
                                                        "Service.getList().subscribe(response => {")
                .NewLine().InsertTab(3).Append("if (response.success) {")
                .NewLine().InsertTab(4)
                .Append($"this.{relationalProperty.Name.ToCamelCase().Pluralize()}= response.result.items;")
                .NewLine().InsertTab(3).Append("} else {")
                .NewLine().InsertTab(4).Append("abp.message.error(response.error.message);")
                .NewLine().InsertTab(3).Append("}")
                .NewLine().InsertTab(2).Append("}, error => abp.message.error(error.error.error.message));")
                .NewLine(2);
        }

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
            .NewLine().InsertTab(3).Append("this.ref.close(respose);")
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


        stringBuilder.Append("}");


        return stringBuilder;
    }

    public static StringBuilder GetCreateComponentHtmlStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }

    public static StringBuilder GetCreateComponentCssStringify()
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }
}