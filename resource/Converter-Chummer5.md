# Chummer5 Converter

Dies definiert einzelne Regeln, die genutzt werden, um eine `.chummer5` Datei zu lesen und in das
interne Format zu überführen.

Chummer5 speichert seine Daten in einem XML Format und nutzt für die Daten nur Knoten und Textfelder
und keine Attribute.

Im Ordner `converter/chummer5` können beliebige `yml` Dateien hinterlegt werden. Diese werden
**alle** nacheinander abgehandelt. Ein Dokument besteht nur aus einen Wurzeleintrag `rules`. In
diesem können dann beliebige Regeln hinterlegt werden.

## Basisregeln

Jede Regel hat die folgenden Felder:

- `path`: Gibt den Pfad im XML Dokument an, der abgelaufen wird.
- `key`: Die Pfad im internen Format wo die Daten hinterlegt werden.
- `type`: Der Wert in dem das umgewandelt werden soll. Gültig ist `number` und `text`.
- `format`: Eine Liste von Regeln die der Textwert vorher umgewandelt werden soll. (Siehe unten)
- `queries`: Eine Liste von Queries. (Siehe unten)
- `templates`: Eine Liste von Template. (Siehe unten)

```yml
rules:
  - path: character.name
    key: bio.meta.name
    type: text
```

## Formatumwandlung

Manche Werte können nicht 1:1 übernommen werden, da Chummer die anders abspeichert (z.B. `Wert m`
oder `Wert / Wert / Wert`). Dazu wird eine Liste von Regeln definiert, die nacheinander den Text
transformieren bevor sie dann im internen Datenformat gespeichert werden.

Jede Regel besteht aus einem `regex` Feld, wo ein regulärer Ausdruck angegeben wird. Diese dürfen
Capture Groups enthalten. Des weiteren enthalten diese ein `text` Feld, wo der Text für den nächsten
Schritt zusammengebaut wird. Hier können Capture Groups mit `{index}` referenziert werden. Capture
Group `{0}` beinhaltet den kompletten Match.

```yml
rules:
  - path: character.height
    key: bio.meta.size
    type: number
    format:
      - regex: (\d+)\.(\d\d)
        text: '{1}{2}'
```

## Queries

Manche Werte werden von Chummer etwas versteckt. Dazu müssen dann in Listen bestimmte Einträge mit
Keys gefunden werden. Am besten erklärt sich dies anhand eines Beispiels:

Die Konstitution wird von Chummer so gespeichert (gekürzt):

```xml
<?xml version="1.0" encoding="utf-8"?>
<character>
    <!--...-->
    <attributes>
        <attribute>
            <name>BOD</name>
            <totalvalue>4</totalvalue>
            <!--...-->
        </attribute>
        <!--...-->
    </attributes>
    <!--...-->
</character>
```

Die Regel sähe dann so aus:

```yml
rules:
  - path: character.attributes
    queries:
      - group: attribute
        search:
          - path: name
            regex: BOD
        path: totalvalue
        key: bio.attr.kon
        type: text
```

Zu aller erst wird bei der Regel der `path` angegeben, wo alle Queries zu finden sind. Dies ist in
diesem Fall `character.attributes`.

Die Liste aller Queries werden in `queries` zusammengefasst. Man kann dies auf mehrere Regeln
aufteilen, aber in `queries` ist es kürzer.

`group` gibt an, welcher Knoten mehrfach wiederholt wird und worin gesucht wird. Dies ist immer ein
Name und kein Pfad.

In `search` werden eine Liste von Bedingungen angegeben, die in dieser Gruppe erfüllt sein müssen.
`path` gibt den Pfad in der Gruppe und `regex` das Regex Muster auf den der Wert passen muss.

Ähnlich zu normalen Regeln gibt dann `path` (innerhalb der Gruppe), `key` und `type` dann den Wert
an, der gesucht werden soll.

Queries können selbst wieder weitere Queries, Templates und Formatierungen enthalten.

## Templates

Für mache Items, Skills, etc. wird im internen Format ein Template angelegt, welche alle Werte
zusammenfasst.

An einem Beispiel sieht das so aus:

```xml
<?xml version="1.0" encoding="utf-8"?>
<character>
    <!--...-->
    <newskills>
        <!--...-->
        <!-- dies ist ein Template, welcher mehrfach angewandt wird -->
        <skills>
            <!--...-->
            <name>Gymnastics</name>
            <!--...-->
        </skills>
        <!--...-->
    </newskills>
    <!--...-->
</character>
```

Und als Regelwerk:

```yml
rules:
  - path: character.newskills
    template:
      name: skills
      root: char.skill
      rules:
        - path: name
          key: name
          type: text
```

Das oberste `path` gibt an, wo alle Templates im XML-Baum lokalisiert sind. `template` gibt dann an,
dass diese Regel als Template zu behandeln ist.

Template hat dann einen `name`, welcher den Namen des XML-Knotens angibt, der wiederholt verwendet
wird. `root` gibt dann den Präfix für alle Werte der internen Repräsentation innerhalb des Templates
an. Außerdem ist dies der Name des Template Schemas.

`rules` gibt dann wieder eine Liste von Regeln an, die alle zu diesem Template dazugehörig sind. Die
Regeln dürfen dann wieder alles enthalten.
