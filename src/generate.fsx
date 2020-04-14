open System
open System.IO

module DllReferencesBuilder =

    let generateReferences includeSystem dllDir  =
        let excepted = 
            [ 
                "FSharp.Data.DesignTime.dll"
                "FSharp.Core.dll"
                "FSharp.Data.dll"
                "Social.ApplicationConfiguration.exe"
            ]
        let system =        
            [            
                @"System.Buffers.dll" 
                @"System.Collections.Immutable.dll" 
                @"System.ComponentModel.Annotations.dll" 
                @"System.Diagnostics.DiagnosticSource.dll" 
                @"System.Interactive.Async.dll" 
                @"System.Memory.dll" 
                @"System.Numerics.Vectors.dll" 
                @"System.Reflection.TypeExtensions.dll" 
                @"System.Runtime.CompilerServices.Unsafe.dll" 
                @"System.Threading.Tasks.Extensions.dll" 
                @"System.ValueTuple.dll" 
         ]
        let allExcepted =
            if not includeSystem then
                Seq.append excepted system |> Seq.toList
            else
                excepted

        let isExcepted file = 
            let fileName = Path.GetFileName(file)  
            allExcepted  |> Seq.contains fileName 
        let isDotNet file =
            try
                System.Reflection.AssemblyName.GetAssemblyName(file) |> ignore
                true
            with
            | :? System.BadImageFormatException -> false
            | :? System.IO.FileLoadException -> true

      
        let highPriorities = ["Newtonsoft"; "May-dotnet"] // Issue with interactive and NewtonsoftAttributes
        let isPrioritized (fileName : string) =
            let name = Path.GetFileName(fileName)
            highPriorities |> Seq.exists(fun i -> name.ToLower().Contains(i.ToLower())) 
        let rDirectives =
            Directory.EnumerateFiles(dllDir)
            |> Seq.filter(fun i -> 
                let ext = Path.GetExtension(i).ToLower()
                ext = ".dll" || ext = ".exe" )
            |> Seq.filter (isExcepted >> not)    
            |> Seq.filter isDotNet
            |> Seq.sortBy(fun n -> match isPrioritized n with |true -> 0 |_-> 1)
            |> Seq.map(fun i ->
                let fileName = Path.GetFileName(i) 

                sprintf """#r @"%s" """ fileName) 

            |> Seq.toList

        rDirectives

let outFile = Path.Combine(__SOURCE_DIRECTORY__, "References.fsx")

let dllDir = fsi.CommandLineArgs |> Seq.last

let includeSystem = fsi.CommandLineArgs |> Seq.contains "--includesystem"
let text = DllReferencesBuilder.generateReferences includeSystem dllDir

File.WriteAllLines(outFile, text)