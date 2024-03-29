function Set-VsoVariable {
    [CmdletBinding()]
    param (
        [parameter(ValueFromPipeline,Mandatory,Position=0)]
        [string]$Name,
        [parameter(Position=1)]
        $Value
    )
    
    begin {
    }
    
    process {
        Write-Host "##vso[task.setvariable variable=$Name;]$Value"
    }
    
    end {
    }
}