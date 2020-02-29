# Unity Versionen

### TODO
* Muss noch die Links sortieren. Glaube bei 5.5 hat er mehrere für verschiedene Editor Varianten.

## Informationen
Falls ich mal wieder eine andere Version brauche, aber keinen Link dazu finde:

### Allgemeines
Ich kann nicht versprechen, dass auch alle Links funktionieren.
Ich habe die mit PowerShell anhand des Hash-Wertes generieren lassen.

Unity Hub Links kann ich leider nicht einfügen. Markdown versteht die Links nicht.

## Tabelle mit Links

UnityVersion | HashValue | HubLink | VersionLink
------------ | --------- | ------- | -----------
2019.3.3 | 7ceaae5f7503 | unityhub://2019.3.3f1/7ceaae5f7503 | [Whats new in Unity 2019.3.3](https://unity3d.com/unity/whats-new/2019.3.3)
2019.3.2 | c46a3a38511e | unityhub://2019.3.2f1/c46a3a38511e | [Whats new in Unity 2019.3.2](https://unity3d.com/unity/whats-new/2019.3.2)
2019.3.1 | 89d6087839c2 | unityhub://2019.3.1f1/89d6087839c2 | [Whats new in Unity 2019.3.1](https://unity3d.com/unity/whats-new/2019.3.1)
2019.3.0 | 27ab2135bccf | unityhub://2019.3.0f6/27ab2135bccf | [Whats new in Unity 2019.3.0](https://unity3d.com/unity/whats-new/2019.3.0)
2019.2.21 | 9d528d026557 | unityhub://2019.2.21f1/9d528d026557 | [Whats new in Unity 2019.2.21](https://unity3d.com/unity/whats-new/2019.2.21)


## Weiteres
### PowerShell Hash Script
```PowerShell
# Remove-Variable AllLinks -ErrorAction SilentlyContinue -Verbose

# TODO: Searching for this?
#https://unity3d.com/de/get-unity/download/archive

# Normal View
#$AllLinks | Select-Object -Property UnityVersion,HashValue,HubLink,VersionLink -Unique

# With Markdown Links
# https://gist.github.com/aaroncalderon/09a2833831c0f3a3bb57fe2224963942
#($AllLinks | Select-Object -Property UnityVersion,HashValue,HubLink,@{N="VersionLink";E={"[Whats new in Unity $($_.UnityVersion)]($($_.VersionLink))"}} -Unique | ConvertTo-Csv -Delimiter "|" -NoTypeInformation) -replace "`"" -replace "[|]", " | "

Write-Verbose "Invoking on 'https://unity3d.com/get-unity/download/archive'" -Verbose
$ArchiveRequest = (Invoke-WebRequest -Uri "https://unity3d.com/get-unity/download/archive")
$Archive_Links  = $ArchiveRequest.Links

# Catch all realease note links form archive page
$ReleaseNoteLinks = $Archive_Links | Where-Object { $_.href -match "new" -and $_.innerText -notlike "*Unity ID*" }

Write-Verbose "Digging through $($ReleaseNoteLinks.Count) links, finding release objects." -Verbose
$OtherLinks = @()

if(!$UnityLinkObjects) {
    $UnityLinkObjects = $ReleaseNoteLinks | ForEach-Object {
        # 2017.4.11 has a space in it?
        $_.href -replace "%20"," " -match "([\d]*[.][\d]*[.|\d]*)"
        $UnityVersion = $Matches[1]
        $UnityReleaseLink = "https://unity3d.com$($_.href)"

        # Fetching release links from release version
        Write-Verbose "Finding current editor and unity hub links for version '$UnityVersion' (ReleaseLink: $UnityReleaseLink)" -Verbose
        $DownloadLinks = $Archive_Links | Where-Object { $_.innerText -match "Editor" -and $_.href -match "$UnityVersion" }

        # Fetching unity hub downloads from release version
        # Note not working with -match "[\/]$UnityVersion" (4.7.2 is find in hex like "541267ge2v")
        $HubLinks = $Archive_Links | Where-Object { $_.innerText -match "Unity Hub" -and $_.href -like "*/$UnityVersion*" }
        if($HubLinks) {
            $HubLinks | ForEach-Object {
                $HubLink = $_.href
                $Found = $Hublink -match "(?!.*\/).+$"
                if($Found) {
                    $HashValue = $Matches[0]
                } else {
                    $HashValue = "Not found"
                }

                # combining all found data
                [PSCustomObject]@{
                    UnityVersion = $UnityVersion
                    VersionLink  = $UnityReleaseLink
                    DownloadLink = ($DownloadLinks.href -join ", ")
                    HashValue = $HashValue
                    HubLink = $HubLink
                }
            }
        } else {
            # just in case I want to add something I do not add later...
            $OtherLinks += [PSCustomObject]@{
                UnityVersion = "$UnityVersion ##O"
                VersionLink  = $UnityReleaseLink
                DownloadLink = $DownloadLinks.href -join ", "
                HashValue = "N/A"
                HubLink = "N/A"
            }
        }
    }

    # Normal Downloads
    $UnityLinkObjects += (Invoke-WebRequest -Uri "https://unity3d.com/unity/whats-new/").Links | `
        Where-Object { $_.href -match "new" -and $_.innerText -match "Unity" -and $_.innerText -notlike "*Unity ID*" } | `
        ForEach-Object {
        $UnityVersion = ($_.innerText -replace "Unity").Trim()
        $UnityVersionLink = $_.href
        #$UnityVersionLink = "https://unity3d.com$UnityVersionLink"

        # only add data not existing yet.
        if($UnityLinkObjects.UnityVersion -contains $UnityVersion) {
            Write-Host "Version $UnityVersion exists."
            return
        } else {
            #Write-Host "Version '$UnityVersion' does not exist in array '$($UnityLinkObjects.UnityVersion -join ", ")'"
        }

        $ChangeLogURI = "https://unity3d.com$UnityVersionLink"
    
        Write-Verbose "Fetching '$ChangeLogURI'; Version: $UnityVersion" -Verbose

        # all download links
        $Links = (Invoke-WebRequest -Uri $ChangeLogURI).Links | Where-Object { $_.innerText -match "Editor|[(]Windows[)]" }
        $Links | ForEach-Object {
            $DownloadText = $_.innerText # Unity Editor
            $DownloadLink = $_.href # download.unity.com

            $Found = $_.href -match "download_unity[\/]([\s\S]*?)[\/]"
            if($Found) {
                $HashValue = $Matches[1]
                $HubLink   = "unityhub://$UnityVersion/$HashValue"
            } else {
                $HashValue = "Not Found"
                $HubLink   = "N/A"
            }

            [PSCustomObject]@{
                UnityVersion = "$UnityVersion ##N"
                VersionLink  = $ChangeLogURI
                DownloadLink = $DownloadLink
                HashValue = $HashValue
                HubLink = $HubLink
            }
        }
    }

    Write-verbose "Invoke on 'https://unity3d.com/unity/beta'"
    $BetaPageLinks = (Invoke-WebRequest -Uri "https://unity3d.com/unity/beta" -UseBasicParsing).Links

    #
    #  Alphas + Betas
    #
    # Working for now. Fetching this link: "https://unity3d.com/de/alpha/2020.1a"
    $LatestAlphaLink = $BetaPageLinks | Where-Object {$_.href -match "alpha" -and $_.innerText -ne "Download"} | Select-Object -First 1
    $LatestAlphaHref = "https://unity3d.com$($LatestAlphaLink.href)"

    Write-Verbose "Fetching 'current' alpha links from '$LatestAlphaHref' ..." -Verbose
    
    $AlphaBetaLinks = (Invoke-WebRequest -Uri $LatestAlphaHref).Links | where {$_.innertext -eq "Download"}
    $AlphaBetaLinks += $BetaPageLinks | Where-Object {$_.outerhtml -match "[>]download" -and !$_.class}

    $UnityLinkObjects += $AlphaBetaLinks | ForEach-Object {
        $UnityVersionLink = $_.href
        $UnityVersion = $_.href -replace "/unity/alpha/" -replace "/unity/beta/"
        
        if($UnityLinkObjects.UnityVersion -contains $UnityVersion) {
            Write-Host "Version $UnityVersion exists."
            return
        } else {
            #Write-Host "Version '$UnityVersion' does not exist in array '$($UnityLinkObjects.UnityVersion -join ", ")'"
        }
        
        $ChangeLogURI = "https://unity3d.com$UnityVersionLink"
        
        Write-Verbose "Fetching '$ChangeLogURI'; Version: $UnityVersion" -Verbose

        # all download links
        $Links = (Invoke-WebRequest -Uri $ChangeLogURI).Links | Where-Object { $_.innerText -match "Editor|[(]Windows[)]" }
        $Links | ForEach-Object {
            $DownloadText = $_.innerText # Unity Editor
            $DownloadLink = $_.href # download.unity.com

            $Found = $_.href -match "download[\/]([\s\S]*?)[\/]" # alpha/beta difference
            if($Found) {
                $HashValue = $Matches[1]
                $HubLink   = "unityhub://$UnityVersion/$HashValue"
            } else {
                $HashValue = "N/A"
                $HubLink   = "N/A"
            }

            [PSCustomObject]@{
                UnityVersion = "$UnityVersion ##A"
                VersionLink  = $ChangeLogURI
                DownloadLink = $DownloadLink
                HashValue = $HashValue
                HubLink = $HubLink
            }
        }
    }

    # Adding what is left:
    $UnityLinkObjects += $OtherLinks | ForEach-Object {
        $UnityVersion = $_.UnityVersion
        if($UnityLinkObjects.UnityVersion -contains $UnityVersion) {
            Write-Verbose "Version $UnityVersion exists." -Verbose
            return
        } else {
            #Write-Host "Version '$UnityVersion' does not exist in array '$($UnityLinkObjects.UnityVersion -join ", ")'"
        }

        Write-Verbose "Adding version $UnityVersion" -Verbose
        $_
    }
}

$UnityLinkObjects | Sort-Object -Property UnityVersion -Descending | Select-Object -Property UnityVersion,HashValue,HubLink,VersionLink,DownloadLink -Unique | FT

# TODO: LTS is missing ...
# https://unity3d.com/unity/qa/lts-releases
```
