# Attributes

> Dies beschreibt wie die `attributes/*.yml` Dateien organisiert sind.

Zu jedem Charakter können eine Liste an Zahlen und Texten verknüpft werden. Diese können in
Kategorien und Gruppen organisiert werden.

## Gruppen

Jede Datei in `resource/attributes/` ist eine Gruppe. Diese kann unter einen gemeinsamen Schlüssel
und als Einheit zusammengefasst werden, indem `root` gesetzt wird.


```yml
# the name of this group
root: foo
# the attributes
attributes:
    number:
        'bar':
            name: Baz
```

In diesem Beispiel ist dies eine `foo` Gruppe und alle Attribute erhalten den Präfix `foo.`. In
diesem Beispiel ist also `Baz` unter dem Schlüssel `foo.bar` zu finden.

Ist `root` nicht gesetzt, so ist dies keine Gruppe und die Attribute bleiben so bestehen.

## Template

Sobald `template: true` gesetzt ist, ist die Gruppe eine Template Gruppe. Dies bedeutet, dass all
ihre Attribute als Template wiederverwendet werden können.

```yml
root: foo
template: true
attributes:
    number:
        'bar':
            name: Baz
```

Die Gruppe aus dem Beispiel lässt sich mehrfach instanzieren. In diesem Beispiel kann bei einer
Instanz von `foo` der Wert `Baz` unter `foo.$x.bar` zu finden sein. `x` ist hierbei die ID der
Instanz `foo`.

IDs werden durch das System automatisch vergeben. Dazu speichert das System unter `foo.@next` den
Wert der nächsten Id und in `foo.@list` alle aktuellen Ids.

## Datentypen

Als Attribute können nur folgende Datentypen genutzt werden:

- `number`: Eine beliebige Int64 Zahl
- `text`: Ein beliebiger String
- `number-list`: Ein Array von Int64 Zahlen

Jede kann verschiedene Attribute mit Einstellungen erhalten. Erlaubt sind:

| Einstellung | Zwingend | Bemerkung |
|-|-|-|
| `name` | x | Der Anzeigename des Attributes |
| `short` | | Ein Kürzel des Attributes. Wird nur bei manchen Anzeigesystemen verwendet |
| `category` | | Die Kategorie unter die diese Attribute zusammengefasst werden können. Wird nur bei manchen Anzeigesystemen verwendet. Gilt nicht Gruppenübergreifend. |
