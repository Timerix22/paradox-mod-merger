CSPROJ_FILE := 'C:/projects/c\#/paradox-mod-merger/paradox-mod-merger/paradox-mod-merger.csproj'
STEAMLIB_DIR := "D:/SteamLibrary/"


DATETIME:=$(shell /usr/bin/date +%Y.%m.%d_%H-%M-%S)

all: pre content post localisation ui_black ui_orange core
# graphics realspace

pre_DIR:=merged/pre_$(DATETIME)
pre:
	@echo ----------------[pre]-----------------
	@if [[ -d merged/pre_latest ]]; then unlink merged/pre_latest; fi
	./merger -merge-subdirs separate/pre -out $(pre_DIR)
	ln -s -r $(pre_DIR) merged/pre_latest

content_DIR:=merged/content_$(DATETIME)
content:
	@echo --------------[content]---------------
	@if [[ -d merged/content_latest ]]; then unlink merged/content_latest; fi
	./merger -merge-subdirs separate/content -out $(content_DIR)
	ln -s -r $(content_DIR) merged/content_latest

post_DIR:=merged/post_$(DATETIME)
post:
	@echo ----------------[post]----------------
	@if [[ -d merged/post_latest ]]; then unlink merged/post_latest; fi
	./merger -merge-subdirs separate/post -out $(post_DIR)
	ln -s -r $(post_DIR) merged/post_latest

localisation_DIR:=merged/localisation_$(DATETIME)
localisation:
	@echo ------------[localisation]------------
	@if [[ -d merged/localisation ]]; then unlink merged/localisation; fi
	./merger -merge-subdirs separate/localisation -out $(localisation_DIR)
	ln -s -r $(localisation_DIR) merged/localisation_latest

realspace_DIR:=merged/realspace_$(DATETIME)
realspace:
	@echo --------------[realspace]-------------
	@if [[ -d merged/realspace_latest ]]; then unlink merged/realspace_latest; fi
	./merger -merge-subdirs separate/realspace -out $(realspace_DIR)
	ln -s -r $(realspace_DIR) merged/realspace_latest


graphics_DIR:=merged/graphics_$(DATETIME)
graphics:
	@echo --------------[graphics]--------------
	@if [[ -d merged/graphics_latest ]]; then unlink merged/graphics_latest; fi
	./merger -merge-subdirs separate/graphics -out $(graphics_DIR)
	ln -s -r $(graphics_DIR) merged/graphics_latest

ui_black_DIR:=merged/ui_black_$(DATETIME)
ui_black:
	@echo --------------[ui_black]--------------
	@if [[ -d merged/ui_black_latest ]]; then unlink merged/ui_black_latest; fi
	./merger -merge-subdirs separate/ui/pre -out $(ui_black_DIR)
	./merger -merge-subdirs separate/ui/black -out $(ui_black_DIR)
	./merger -merge-subdirs separate/ui/post -out $(ui_black_DIR)
	ln -s -r $(ui_black_DIR) merged/ui_black_latest

ui_orange_DIR:=merged/ui_orange_$(DATETIME)
ui_orange:
	@echo --------------[ui_orange]-------------
	@if [[ -d merged/ui_orange_latest ]]; then unlink merged/ui_orange_latest; fi
	./merger -merge-subdirs separate/ui/pre -out $(ui_orange_DIR)
	./merger -merge-subdirs separate/ui/orange -out $(ui_orange_DIR)
	./merger -merge-subdirs separate/ui/post -out $(ui_orange_DIR)
	ln -s -r $(ui_orange_DIR) merged/ui_orange_latest

core_DIR:=merged/core_$(DATETIME)
core:
	@echo ----------------[core]----------------
	@if [[ -d merged/core_latest ]]; then unlink merged/core_latest; fi
	mkdir $(core_DIR)
	cp -r merged/pre_latest/* $(core_DIR)/
	./merger -merge-single merged/content_latest -out $(core_DIR)
	./merger -merge-single merged/post_latest -out $(core_DIR)
	./merger -merge-single merged/localisation_latest -out $(core_DIR)
	#./merger -merge-single merged/realspace_latest -out $(core_DIR)
	ln -s -r $(core_DIR) merged/core_latest

clean:
	rm -rf merged
	rm -rf logs
	rm -rf conflicts

clear_workshop:
	./merger -clear $(STEAMLIB_DIR)/steamapps/workshop/content/281990 -out src/src_$(DATETIME)

update_merger:
	rm -rf paradox-mod-merger
	dotnet publish $(CSPROJ_FILE) -o ./paradox-mod-merger -c release -f net7.0
	echo '#!/bin/bash' > merger
	echo 'paradox-mod-merger/paradox-mod-merger.exe "$$@"' >> merger
	chmod +x merger

create_dirs:
	mkdir -p src
	mkdir -p separate
	mkdir -p separate/
	mkdir -p separate/content
	mkdir -p separate/graphics
	mkdir -p separate/localisation
	mkdir -p separate/post
	mkdir -p separate/pre
	mkdir -p separate/ui
	mkdir -p separate/ui/pre
	mkdir -p separate/ui/black
	mkdir -p separate/ui/orange
	mkdir -p separate/ui/post
	mkdir -p separate/unused
