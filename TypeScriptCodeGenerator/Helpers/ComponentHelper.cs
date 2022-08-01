using System.Text;
using Extensions;
using Humanizer;

namespace TypeScriptCodeGenerator.Helpers;

public static class ComponentHelper
{
    public static StringBuilder GetComponentTsStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();

        //Imports
        stringBuilder.Append("import {Component, Injector, OnInit} from '@angular/core';")
            .NewLine()
            .Append("import {appModuleAnimation} from '../../../shared/animations/routerTransition';")
            .NewLine()
            .Append("import {AppComponentBase} from '../../../shared/app-component-base';")
            .NewLine()
            .Append("import {" + entity.Name + "Service} from '../../../shared/services/" + entity.Name + "/" +
                    entity.Name.ToCamelCase() + ".service';")
            .NewLine()
            .Append("import {" + entity.Name + "FullOutput} from '../../../shared/services/" + entity.Name + "/dtos/" +
                    entity.Name + "FullOutput';")
            .NewLine()
            .Append("import {DataGridOptions} from '../../../shared/components/dataGrid/model/dataGridOptions';")
            .NewLine()
            .Append("import {GridColumn} from '../../../shared/components/dataGrid/model/gridColumn';")
            .NewLine()
            .Append("import {DialogOptions} from '../../../shared/components/dataGrid/model/dialogOptions';")
            .NewLine()
            .Append("import {Create" + entity.Name + "Component} from './create-" + entity.Name.ToCamelCase() +
                    "/create-" + entity.Name.ToCamelCase() + ".component';")
            .NewLine()
            .Append("import {Title} from '@angular/platform-browser';")
            .NewLine(2);

        //Component
        stringBuilder.Append("@Component({")
            .NewLine()
            .InsertTab().Append($"selector: 'app-{entity.Name.ToCamelCase()}',")
            .NewLine()
            .InsertTab().Append($"templateUrl: './{entity.Name.ToCamelCase()}.component.html',")
            .NewLine()
            .InsertTab().Append($"styleUrls: ['./{entity.Name.ToCamelCase()}.component.css'],")
            .NewLine()
            .InsertTab().Append($"animations: [appModuleAnimation()],")
            .NewLine().Append("})")
            .NewLine();

        stringBuilder.Append($"export class {entity.Name}Component extends AppComponentBase implements OnInit ")
            .Append("{")
            .NewLine();

        stringBuilder.Append(@$"    loading: boolean = true;
    dataGridOptions: DataGridOptions = new DataGridOptions();
    dataGridColumns: GridColumn[];

    dataSource = new Array<{entity.Name}FullOutput>();
    globalFilterFields: string[];
    exportFields: string[];

    actionOps: any[];").NewLine(2);


        stringBuilder.InsertTab().Append("constructor(injector: Injector,")
            .NewLine().InsertTab(4).Append("private titleService: Title, ")
            .NewLine().InsertTab(4).Append($"public {entity.Name.ToCamelCase()}Service: {entity.Name}Service) ")
            .Append("{")
            .NewLine()
            .InsertTab(2).Append("super(injector);")
            .NewLine()
            .InsertTab(2).Append($"titleService.setTitle('{entity.ProjectDisplayName} | ' + this.l('{entity.Name}'));")
            .NewLine()
            .InsertTab().Append("}")
            .NewLine();

        stringBuilder.NewLine()
            .InsertTab().Append("ngOnInit(): void {")
            .NewLine();


        stringBuilder.InsertTab(2).Append($"this.{entity.Name.ToCamelCase()}Service.getList().subscribe((response) => ")
            .Append("{")
            .NewLine()
            .InsertTab(4).Append($"this.dataSource = new Array<{entity.Name}FullOutput>();")
            .NewLine()
            .InsertTab(4).Append("this.dataSource = response.result.items;")
            .NewLine().InsertTab(4).Append("this.initializeDataGridOptions();")
            .NewLine().InsertTab(3).Append("},")
            .NewLine().InsertTab(3).Append("(error) => {")
            .NewLine().InsertTab(4).Append($"this.dataSource = new Array<{entity.Name}FullOutput>();")
            .NewLine().InsertTab(4).Append("this.initializeDataGridOptions();")
            .NewLine().InsertTab(3).Append("});")
            .NewLine().InsertTab().Append("}")
            .NewLine();


        stringBuilder.Append(@"    initializeDataGridOptions() {
        //Services
        this.dataGridOptions.crudService = this.{{EntityNameCc}}Service;

        //Data Source & Components
        this.dataGridOptions.keyExpr = 'id';
        this.dataGridOptions.dataSource = this.dataSource;
        this.dataGridOptions.columns = this.initializeDataGridColumns();
        this.dataGridOptions.exportCols = this.exportFields;
        this.dataGridOptions.createComponent = Create{{EntityName}}Component;
        //this.dataGridOptions.editComponent = Edit{{EntityName}}Component;
        let dialogOptions: DialogOptions = new DialogOptions();
        this.dataGridOptions.dialogConfig = dialogOptions;

        //Permissions
        this.dataGridOptions.createPermissionName = '{{EntityName}}.Create';
        this.dataGridOptions.editPermissionName = '{{EntityName}}.Update';
        this.dataGridOptions.deletePermissionName = '{{EntityName}}.Delete';

        //Button Controls
        this.dataGridOptions.createButtonIsActive = true;
        this.dataGridOptions.createButtonIsHidden = false;

        this.dataGridOptions.importButtonIsActive = false;
        this.dataGridOptions.importButtonIsHidden = true;

        this.dataGridOptions.exportButtonIsActive = true;
        this.dataGridOptions.deleteButtonIsActive = true;
        this.dataGridOptions.editButtonIsActive = true;

        //Header & Footer Text
        this.dataGridOptions.entityName = '{{EntityName}}';
        this.dataGridOptions.currentPageReportTemplate = 'Showing {first} to {last} of {totalRecords} entries';

        //Pagination
        this.dataGridOptions.paginator = true;
        this.dataGridOptions.pageSize = '10';
        this.dataGridOptions.pageSizes = [5, 10, 20];
        this.dataGridOptions.pageReport = true;

        //Sorting
        this.dataGridOptions.sortingMode = 'multiple';

        //Filtering
        this.dataGridOptions.globalFilter = true;
        this.dataGridOptions.globalFilters = this.globalFilterFields;
        this.dataGridOptions.globalFilterWidth = '240px';

        //Toast Settings
        this.dataGridOptions.toastPosition = 'bottom-right';
        this.dataGridOptions.toastLife = 3000;
        this.dataGridOptions.toastSticky = false;
        this.dataGridOptions.toastClosable = true;

        //Grid Styling
        this.dataGridOptions.showBorders = false;
        this.dataGridOptions.rowHover = true;
        this.dataGridOptions.scrollable = true;

        //Selection
        this.dataGridOptions.selectionEnabled = false;
        this.dataGridOptions.selection = 'single';
        this.dataGridOptions.selectedItems = null;

        //Grid Column Config
        this.dataGridOptions.allowColumnReordering = true;
        this.dataGridOptions.allowColumnResizing = true;
        this.dataGridOptions.allowcolumnAutoWidth = true;
    }".Replace("{{EntityName}}", entity.Name).Replace("{{EntityNameCc}}", entity.Name.ToCamelCase()));


        stringBuilder.NewLine(2).InsertTab().Append("initializeDataGridColumns(): GridColumn[] {")
            .NewLine().InsertTab(2).Append("this.dataGridColumns = [").NewLine();

        stringBuilder.Append(@"            new GridColumn(
                'id',
                'Id',
                'numeric',
                '',
                'right',
                false,
                '',
                '',
                false,
                true,
                false,
                []
            ),").NewLine();

        foreach (var property in entity.Properties.Where(x => !x.IsRelationalProperty))
        {
            stringBuilder.Append($@"            new GridColumn(
                '{property.Name.ToCamelCase()}',
                '{property.Name.ToTitle()}',
                '{property.Type.ToTypeScriptDataGridColumnType()}',
                '',
                'right',
                false,
                '',
                '',
                false,
                true,
                true,
                []
            ),
");
        }

        stringBuilder.InsertTab(2).Append("];")
            .NewLine().InsertTab(2).Append("return this.dataGridColumns;")
            .NewLine().InsertTab().Append("}").NewLine();

        stringBuilder.Append("}");
        return stringBuilder;
    }

    public static StringBuilder GetComponentHtmlStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div [@routerTransition]>")
            .NewLine().InsertTab()
            .Append(
                $"<app-dataGrid [dataGridOptionInput]=\"dataGridOptions\" [crudService]=\"{entity.Name.ToCamelCase()}Service\"></app-dataGrid>")
            .NewLine().Append("</div>");
        return stringBuilder;
    }

    public static StringBuilder GetComponentCssStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }
}