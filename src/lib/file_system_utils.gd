class_name FileSystemUtils


static func map_files(path: String, callable: Callable) -> void:

	var directory = DirAccess.open(path)

	assert(directory != null, "Directory not found: %s" % path)

	directory.list_dir_begin()

	var file_name = directory.get_next()

	while file_name != "":

		if directory.current_is_dir():
			continue

		callable.call(path.path_join(file_name))
		file_name = directory.get_next()
