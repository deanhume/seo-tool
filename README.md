<img src="logo.jpg" alt="seo tool logo" width="400"/>

## SEO TOOL

This is a cheeky little SEO tool that takes in a sitemap.xml file and loops through all the URLs.

It does the following:
- Check the external links on the page to see if they are valid.
- Check the image links on the page to ensure that they are valid.

It then outputs to results to the console.

### TODO
- The sitemap.xml path is hardcoded. Ideally, this will be passed through as a parameter in the console.
- The ignore URL list is hardcoded. In the future, make this read from a txt/csv file.
- Add a check for images and see if they have alt tags.