using System.Text;
using Extensions;

namespace TypeScriptCodeGenerator.Helpers;

public static class ComponentHelper
{
    public static StringBuilder GetComponentTsStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("import {Component, Injector, OnInit} from '@angular/core';")
            .NewLine()
            .Append("import {appModuleAnimation} from '../../../shared/animations/routerTransition';")
            .NewLine()
            .Append("import {AppComponentBase} from '../../../shared/app-component-base';")
            .NewLine()
            .Append("import {" + entity.Name + "Service} from '../../../shared/services/" + entity.Name + "/" +
                    entity.Name.ToCamelCase() + ".service';")
            .NewLine(2);

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
            .NewLine(2)
            .InsertTab().Append("constructor(injector: Injector,")
            .NewLine().InsertTab(4).Append($"private {entity.Name.ToCamelCase()}Service: {entity.Name}Service) ")
            .Append("{")
            .NewLine()
            .InsertTab(2).Append("super(injector);")
            .NewLine()
            .InsertTab().Append("}")
            .NewLine();

        stringBuilder.NewLine()
            .InsertTab().Append("ngOnInit(): void {")
            .NewLine()
            .InsertTab().Append("}")
            .NewLine();

        stringBuilder.Append("}");
        return stringBuilder;
    }

    public static StringBuilder GetComponentHtmlStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"<p>{entity.Name} component works!</p>");
        return stringBuilder;
    }

    public static StringBuilder GetComponentCssStringify(Entity entity)
    {
        var stringBuilder = new StringBuilder();
        return stringBuilder;
    }
}